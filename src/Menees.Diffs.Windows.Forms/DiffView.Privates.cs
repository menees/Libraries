namespace Menees.Diffs.Windows.Forms
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using System.Text;
	using System.Windows.Forms;
	using Menees.Diffs.Windows.Forms;
	using Menees.Windows.Forms;

	#endregion

	internal sealed partial class DiffView : Control
	{
		#region Private Data Members

		private const int CaretWidth = 2;
		private const int GutterSeparatorWidth = 2;
		private const TextFormatFlags DefaultTextFormat =
			TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform |
			TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.TextBoxControl;

		private static readonly Size MaxSize = new(int.MaxValue, int.MaxValue);

		private readonly BorderStyle borderStyle = BorderStyle.Fixed3D;
		private readonly Timer autoScrollTimer;
		private bool capturedMouse;
		private bool showWhitespace;
		private Caret? caret;
		private int charWidth = 1;
		private int gutterWidth = 1;
		private int horizontalAutoScrollAmount;
		private int lineHeight = 1;
		private int verticalAutoScrollAmount;
		private int wheelDelta;
		private DiffViewLines? lines;
		private DiffViewPosition position;
		private DiffViewPosition selectionStart = DiffViewPosition.Empty;
		private string gutterFormat = "{0}";

		#endregion

		#region Private Enums

		private enum TokenCharacterType
		{
			// We haven't determined the character type yet.
			Unknown,

			// A letter, number, or underscore
			Identifier,

			// Whitespace
			Whitespace,

			// Another non-whitespace, non-identifier character
			Other,
		}

		#endregion

		#region Private Methods

		private static bool MatchTokenCharacterType(char ch, ref TokenCharacterType charTypeToMatch)
		{
			TokenCharacterType charType = TokenCharacterType.Other;
			if (ch == '_' || char.IsLetterOrDigit(ch))
			{
				charType = TokenCharacterType.Identifier;
			}
			else if (char.IsWhiteSpace(ch))
			{
				charType = TokenCharacterType.Whitespace;
			}

			bool result;
			if (charTypeToMatch == TokenCharacterType.Unknown)
			{
				// On the first character, we'll only return false if the type was whitespace.
				charTypeToMatch = charType;
				result = charType != TokenCharacterType.Whitespace;
			}
			else
			{
				result = charType == charTypeToMatch;
			}

			return result;
		}

		private static int MeasureString(Graphics g, string displayText, int length, Font font)
		{
			// The caller should pass display text, so that tabs are expanded to spaces properly.
			string segment = displayText.Substring(0, length);
			Size result = TextRenderer.MeasureText(g, segment, font, MaxSize, DefaultTextFormat);
			return result.Width;
		}

		private void AutoScrollTimer_Tick(object? sender, EventArgs e)
		{
			this.VScrollPos += this.verticalAutoScrollAmount;
			this.HScrollPos += this.horizontalAutoScrollAmount;

			// Set the selection end
			Point point = this.PointToClient(Control.MousePosition);
			DiffViewPosition pos = this.GetPosFromPoint(point.X, point.Y);
			this.SetSelectionEnd(pos.Line, pos.Column);
		}

		private void ClearSelection()
		{
			if (this.HasSelection)
			{
				this.InvalidateSelection();
				this.selectionStart = DiffViewPosition.Empty;
				this.FireSelectionChanged();
			}
		}

		private void DiffOptionsChanged(object? sender, EventArgs e)
		{
			// The colors and/or tab width changed.
			this.UpdateTextMetrics(true);

			// If the tab width changed, we have to recalculate the scroll boundaries based on string lengths.
			this.SetupScrollBars();

			// Invalidating the whole window will take care of the color change.
			this.Invalidate();
		}

		private void DrawBackground(Graphics g, SolidBrush brush, int y, bool deadSpace)
		{
			if (deadSpace)
			{
				using (Brush? deadBrush = DiffOptions.TryCreateDeadSpaceBrush(brush.Color))
				{
					// If hatching is turned off, then we have to fallback to the solid brush.
					g.FillRectangle(deadBrush ?? brush, this.gutterWidth, y, this.ClientSize.Width, this.lineHeight);
				}
			}
			else
			{
				g.FillRectangle(brush, this.gutterWidth, y, this.ClientSize.Width, this.lineHeight);
			}
		}

		private void DrawChangedLineBackground(
			Graphics g,
			DisplayLine displayLine,
			string displayText,
			EditScript changeEditScript,
			bool useA,
			int x,
			int y)
		{
			IList<Segment> segments = displayLine.GetChangeSegments(changeEditScript, useA);

			// If the change only inserted or only deleted chars, then one side will have changes but the other won't.
			if (segments.Count > 0)
			{
				// The main line background has already been drawn, so we just
				// need to draw the deleted or inserted background segments.
				Color changeColor = DiffOptions.GetColorForEditType(useA ? EditType.Delete : EditType.Insert);
				using (Brush changeBrush = new SolidBrush(changeColor))
				{
					Font font = this.Font;
					foreach (Segment segment in segments)
					{
						int segStartX = MeasureString(g, displayText, segment.Start, font);
						int segEndX = MeasureString(g, displayText, segment.Start + segment.Length, font);
						g.FillRectangle(changeBrush, x + segStartX, y, segEndX - segStartX, this.lineHeight);
					}
				}
			}
		}

		private void DrawGutter(
			Graphics g,
			SolidBrush fontBrush,
			SolidBrush backBrush,
			bool hasFocus,
			int lineNumber,
			DiffViewLine line,
			int y,
			Brush gutterBrush,
			int lineNumIndent)
		{
			// Draw the gutter background
			backBrush.Color = this.BackColor;

			Brush lineGutterBrush = gutterBrush;
			Brush lineFontBrush = fontBrush;

			if (lineNumber == this.position.Line && hasFocus)
			{
				lineGutterBrush = SystemBrushes.Highlight;
				lineFontBrush = SystemBrushes.HighlightText;
			}

			this.DrawGutterBackground(g, lineGutterBrush, backBrush, y);

			// Draw the line number (as 1-based)
			if (line.Number.HasValue)
			{
				this.DrawString(g, string.Format(this.gutterFormat, line.Number.Value + 1), lineFontBrush, lineNumIndent, y);
			}
		}

		private void DrawGutterBackground(Graphics g, Brush gutterBrush, Brush backBrush, int y)
		{
			int darkWidth = this.gutterWidth - GutterSeparatorWidth;
			g.FillRectangle(gutterBrush, 0, y, darkWidth, this.lineHeight);
			g.FillRectangle(backBrush, darkWidth, y, GutterSeparatorWidth, this.lineHeight);
			g.DrawLine(SystemPens.ControlDark, darkWidth - 1, y, darkWidth - 1, y + this.lineHeight);
		}

		private void DrawLine(
			Graphics g,
			SolidBrush fontBrush,
			SolidBrush backBrush,
			bool hasFocus,
			int lineNumber,
			DiffViewLine line,
			int x,
			int y,
			bool hasSelection,
			DiffViewPosition startSel,
			DiffViewPosition endSel)
		{
			DisplayLine displayLine = this.GetDisplayLine(line);
			string lineText = displayLine.GetDisplayText();

			// If any portion of the line is selected, we have to paint that too.
			int selStartX = 0, selEndX = 0;
			bool lineHasSelection = false;
			bool lineFullySelected = false;
			if (hasSelection && lineNumber >= startSel.Line && lineNumber <= endSel.Line)
			{
				int lineLength = lineText.Length;
				int selStartColumn = (lineNumber == startSel.Line) ? startSel.Column : 0;
				int selEndColumn = (lineNumber == endSel.Line) ? Math.Min(endSel.Column, lineLength) : lineLength;

				lineHasSelection = true;
				lineFullySelected = (lineNumber > startSel.Line && lineNumber < endSel.Line) || (selStartColumn == 0 && selEndColumn == lineLength);

				selStartX = this.GetXForColumn(g, displayLine, lineText, selStartColumn);
				selEndX = this.GetXForColumn(g, displayLine, lineText, selEndColumn);
			}

			// Draw the background.  Even if the line is completely selected,
			// we want to do this because after the last char, we don't paint
			// with the selection color.  So it needs to be the normal back color.
			backBrush.Color = line.Edited ? DiffOptions.GetColorForEditType(line.EditType) : this.BackColor;
			this.DrawBackground(g, backBrush, y, !line.Number.HasValue);

			// Draw the line text if any portion of it is unselected.
			if (!lineFullySelected)
			{
				// The DiffViewLine will cache the edit script, but I'm intentionally not
				// pulling it until we have to have it for rendering.  Getting intra-line
				// diffs makes the whole process into an O(n^2) operation instead of
				// just an O(n) operation for line-by-line diffs.  So I'm try to defer the
				// extra work until the user requests to see the changed line.  It's still
				// the same amount of work if they view every line, but it makes the
				// user interface more responsive to split it up like this.
				EditScript? changeEditScript = line.GetChangeEditScript(this.ChangeDiffOptions);
				if (changeEditScript != null)
				{
					this.DrawChangedLineBackground(g, displayLine, lineText, changeEditScript, line.FromA, x, y);
				}

				this.DrawString(g, lineText, fontBrush, x, y);
			}

			// Draw the selection
			if (lineHasSelection)
			{
				// Draw the background
				RectangleF r = new(selStartX, y, selEndX - selStartX, this.lineHeight);
				Brush brush = hasFocus ? SystemBrushes.Highlight : SystemBrushes.Control;
				g.FillRectangle(brush, r);

				// Draw the selected text.  This draws the string from the original X, but it
				// changes the clipping region so that only the portion inside the highlighted
				// rectangle will paint with the selected text color.
				Region originalClipRegion = g.Clip;
				using (Region textClip = new(r))
				{
					g.Clip = textClip;
					brush = hasFocus ? SystemBrushes.HighlightText : SystemBrushes.WindowText;
					this.DrawString(g, lineText, brush, x, y);
					g.Clip = originalClipRegion;
				}
			}
		}

		private void DrawString(Graphics g, string displayText, Brush brush, int x, int y)
		{
			// The caller should pass display text, so that tabs are expanded to spaces properly.
			//
			// DrawText has some type of limit around 32K pixels where it won't draw and won't raise an error.
			// When using Consolas 10pt, I've only seen this problem with lines over 4800 characters long.
			// So I'll check for this if the text is "long" and then manually draw the text in two smaller parts.
			// I'll set my "long" length to a fraction of 4800, so we won't do extra measuring very often.
			// Note: This could still have a problem if someone uses "long" lines with an 80pt font, but that
			// should be extremely rare.
			const int LongLineLength = 600;
			const int GdiWidthLimit = 32000;
			int displayTextLength = displayText.Length;
			if (displayTextLength < LongLineLength || MeasureString(g, displayText, displayTextLength, this.Font) < GdiWidthLimit)
			{
				TextRenderer.DrawText(g, displayText, this.Font, new Point(x, y), ((SolidBrush)brush).Color, DefaultTextFormat);
			}
			else
			{
				int splitIndex = displayTextLength / 2;
				string leftText = displayText.Substring(0, splitIndex);
				string rightText = displayText.Substring(splitIndex);
				this.DrawString(g, leftText, brush, x, y);
				int measuredWidth = MeasureString(g, leftText, leftText.Length, this.Font);
				this.DrawString(g, rightText, brush, x + measuredWidth, y);
			}
		}

		private void ExtendSelection(int lines, int columns)
		{
			int line = this.position.Line + lines;
			int column = this.position.Column + columns;
			this.SetSelectionEnd(line, column);
		}

		private int FindCurrentTokenEnd(DiffViewPosition start, bool includeTrailingWhitespace)
		{
			DisplayLine displayLine = this.GetDisplayLine(start.Line);
			int startTextIndex = displayLine.GetTextIndexFromDisplayColumn(start.Column);
			string lineText = displayLine.OriginalText;
			int lineTextLength = lineText.Length;

			int endTextIndex = startTextIndex;
			TokenCharacterType charTypeToMatch = TokenCharacterType.Unknown;
			while (endTextIndex < lineTextLength)
			{
				char ch = lineText[endTextIndex];
				if (!MatchTokenCharacterType(ch, ref charTypeToMatch))
				{
					break;
				}

				endTextIndex++;
			}

			if (!includeTrailingWhitespace && endTextIndex == startTextIndex)
			{
				// If we didn't move, then we should skip over any upcoming whitespace
				// rather than just skipping one whitespace character with the logic below.
				includeTrailingWhitespace = true;
			}

			if (includeTrailingWhitespace)
			{
				while (endTextIndex < lineTextLength && char.IsWhiteSpace(lineText[endTextIndex]))
				{
					endTextIndex++;
				}
			}

			if (endTextIndex == startTextIndex)
			{
				// Make sure we move forward at least one character.
				endTextIndex++;
			}

			int result = displayLine.GetDisplayColumnFromTextIndex(endTextIndex);
			return result;
		}

		private int FindCurrentTokenStart(DiffViewPosition start, bool allowStartAsResult)
		{
			DisplayLine displayLine = this.GetDisplayLine(start.Line);
			int originalStartTextIndex = displayLine.GetTextIndexFromDisplayColumn(start.Column);
			string lineText = displayLine.OriginalText;
			int lineTextLength = lineText.Length;

			int startTextIndex = originalStartTextIndex;
			if (lineTextLength > 0)
			{
				if (!allowStartAsResult)
				{
					// Skip any "leading" whitespace (but we're going in reverse, so it's actually trailing).
					while (startTextIndex > 0 && startTextIndex < lineTextLength && char.IsWhiteSpace(lineText[startTextIndex - 1]))
					{
						startTextIndex--;
					}
				}

				TokenCharacterType charTypeToMatch = TokenCharacterType.Unknown;
				while (startTextIndex > 0)
				{
					// Look at the previous character.
					char prevChar = lineText[startTextIndex - 1];
					if (!MatchTokenCharacterType(prevChar, ref charTypeToMatch))
					{
						break;
					}

					startTextIndex--;
				}
			}

			if (startTextIndex == originalStartTextIndex && !allowStartAsResult)
			{
				// Make sure we move back at least one character.
				startTextIndex--;
			}

			int result = displayLine.GetDisplayColumnFromTextIndex(startTextIndex);
			return result;
		}

		private void FireSelectionChanged()
		{
			this.SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		private DisplayLine GetDisplayLine(int line)
		{
			DisplayLine result;
			if (line >= 0 && line < this.LineCount && this.lines != null)
			{
				result = this.GetDisplayLine(this.lines[line]);
			}
			else
			{
				result = this.GetDisplayLine(DiffViewLine.Empty);
			}

			return result;
		}

		private DisplayLine GetDisplayLine(DiffViewLine line)
		{
			DisplayLine result = new(line, this.showWhitespace, DiffOptions.SpacesPerTab);
			return result;
		}

		private int GetDisplayLineLength(int line)
		{
			DisplayLine displayLine = this.GetDisplayLine(line);
			int result = displayLine.GetDisplayTextLength();
			return result;
		}

		private DiffViewPosition GetDocumentEndPosition()
		{
			int line = Math.Max(this.LineCount - 1, 0);
			int column = this.GetDisplayLineLength(line);
			return new DiffViewPosition(line, column);
		}

		private DiffViewPosition GetFindStartPosition(bool searchUp)
		{
			DiffViewPosition result = this.position;

			if (this.HasSelection)
			{
				this.GetForwardOrderSelection(out DiffViewPosition startSel, out DiffViewPosition endSel);

				if (searchUp)
				{
					result = startSel;
				}
				else
				{
					result = endSel;
				}
			}

			return result;
		}

		private void GetForwardOrderSelection(out DiffViewPosition startSel, out DiffViewPosition endSel)
		{
			// Determine the selection positions in forward order.
			// Get them in order in case we have a reverse selection.
			startSel = this.selectionStart;
			endSel = this.position;
			if (startSel.Line > endSel.Line || (startSel.Line == endSel.Line && startSel.Column > endSel.Column))
			{
				startSel = this.position;
				endSel = this.selectionStart;
			}
		}

		private bool GetSingleLineSelectedText(out string? text)
		{
			text = null;
			bool result = false;

			if (this.HasSelection)
			{
				this.GetForwardOrderSelection(out DiffViewPosition startSel, out DiffViewPosition endSel);
				if (startSel.Line == endSel.Line && endSel.Column > startSel.Column)
				{
					DisplayLine displayLine = this.GetDisplayLine(startSel.Line);
					text = displayLine.GetTextBetweenDisplayColumns(startSel.Column, endSel.Column);
					result = true;
				}
			}

			return result;
		}

		private int GetXForColumn(Graphics g, DisplayLine displayLine, string? displayText, int column)
		{
			if (displayText == null)
			{
				displayText = displayLine.GetDisplayText();
			}

			int length = Math.Max(0, Math.Min(displayText.Length, column));

			// Make sure the column's X position is always shifted left to the
			// start of the character that contains/crosses the column.
			length = displayLine.GetColumnStart(length);

			int x = MeasureString(g, displayText, length, this.Font);
			int result = x - (this.HScrollPos * this.charWidth) + this.gutterWidth;
			return result;
		}

		private void GoToPosition(int line, int column)
		{
			// If requested line is on the screen and not in the last 3 lines, then go to it without re-centering.
			// I picked 3 because that usually gets a reasonable amount of context information on the screen
			// for most differences.
			int firstVisibleLine = this.FirstVisibleLine;
			int lastVisibleLine = firstVisibleLine + this.VisibleLineCount - 1;
			const int ContextLines = 3;
			if (line < firstVisibleLine || line > (lastVisibleLine - ContextLines))
			{
				this.CenterVisibleLine = line;
			}

			this.SetPosition(line, column);
		}

		private bool InSelection(DiffViewPosition pos)
		{
			bool result = false;

			if (this.HasSelection)
			{
				this.GetForwardOrderSelection(out DiffViewPosition startSel, out DiffViewPosition endSel);
				result = pos >= startSel && pos <= endSel;
			}

			return result;
		}

		private void InvalidateCaretGutter()
		{
			// Invalidate the gutter portion for the line with the caret.
			Point point = this.GetPointFromPos(this.position.Line, 0);
			Rectangle r = new(0, point.Y, this.gutterWidth, this.lineHeight);
			this.Invalidate(r);
		}

		private void InvalidateSelection()
		{
			if (this.HasSelection)
			{
				int firstLine = Math.Min(this.selectionStart.Line, this.position.Line);
				Point point = this.GetPointFromPos(firstLine, 0);
				int numLines = Math.Abs(this.selectionStart.Line - this.position.Line) + 1;
				Rectangle r = new(this.gutterWidth, point.Y, this.ClientSize.Width, numLines * this.lineHeight);
				this.Invalidate(r);
			}
		}

		private void OffsetPosition(int lines, int columns)
		{
			int line = this.position.Line + lines;
			int column = this.position.Column + columns;
			this.SetPosition(line, column);
		}

		private void ReleaseCaret()
		{
			// Sometimes in the debugger, the IDE seems to steal focus, and the OnGot/LostFocus event can
			// fire twice without us receiving the matching OnLost/GotFocus call.  So we have to protect against
			// that here and check to see if we still have the caret.  We'll clean it up and recreate it when needed
			// because that seems safer than trying to reuse an existing caret if it's left over from a weird
			// intermediate state that shouldn't be happening normally.
			if (this.caret != null)
			{
				this.caret.Dispose();
				this.caret = null;
			}
		}

		private void ScrollHorizontally(int newPos, int originalPos)
		{
			int pos = this.UpdateScrollPos(newPos, true);
			if (pos != originalPos)
			{
				// Don't scroll the line number gutter
				Rectangle r = this.ClientRectangle;
				r.Offset(this.gutterWidth, 0);

				// This really seems like it should be necessary,
				// but if I do it then a GutterWidth sized band
				// is skipped on the scrolling/invalidated end...
				// r.Width -= GutterWidth;
				int numPixels = this.charWidth * (originalPos - pos);
				if (Math.Abs(numPixels) < this.ClientSize.Width)
				{
					// Scroll the subset of the window in the clipping region.
					//
					// Note: We must scroll by the integral this.CharWidth.  Otherwise,
					// round off causes pixel columns to occasionally get dropped
					// or duplicated.  This makes for ugly text until the next full
					// repaint.  By always using the same integral this.CharWidth, the
					// text scrolls smoothly and correctly.
					//
					// To make this smooth and correct, we also set the scroll bar
					// Page size and calculate X in OnPaint using the integral
					// this.CharWidth.
					NativeMethods.ScrollWindow(this, numPixels, 0, ref r, ref r);
				}
				else
				{
					this.Invalidate(r);
				}

				// ScrollWindow is supposed to update the caret position too,
				// but it doesn't when a rect and clipping rect are specified.
				// So we have to update it manually.  This is also necessary
				// because we don't ever want the caret to display in the gutter.
				this.UpdateCaret();

				this.HScrollPosChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private void ScrollVertically(int newPos, int originalPos)
		{
			int pos = this.UpdateScrollPos(newPos, false);
			if (pos != originalPos)
			{
				int numPixels = this.lineHeight * (originalPos - pos);
				if (numPixels < this.ClientSize.Height)
				{
					NativeMethods.ScrollWindow(this, 0, numPixels);
				}
				else
				{
					this.Invalidate();

					// We have to manually update the caret if we don't call ScrollWindow.
					this.UpdateCaret();
				}

				this.VScrollPosChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private void SetPosition(int line, int column)
		{
			this.SetPosition(line, column, true, true);
		}

		private void SetPosition(int line, int column, bool clearSelection, bool scrollToCaret)
		{
			line = EnsureInRange(0, line, this.LineCount - 1);

			DisplayLine displayLine = this.GetDisplayLine(line);
			int length = displayLine.GetDisplayTextLength();
			column = EnsureInRange(0, column, length);

			// Align the column to the start of the character it overlaps (in case it's inside a tab).
			column = displayLine.GetColumnStart(column);

			if (clearSelection)
			{
				this.ClearSelection();
			}

			bool lineNumberChanged = this.position.Line != line;
			bool columnNumberChanged = this.position.Column != column;

			if (lineNumberChanged || columnNumberChanged)
			{
				// Invalidate the old gutter line.
				if (lineNumberChanged)
				{
					this.InvalidateCaretGutter();
				}

				this.position = new DiffViewPosition(line, column);

				// Invalidate the new gutter line.
				if (lineNumberChanged)
				{
					this.InvalidateCaretGutter();
				}

				this.UpdateCaret();

				// If the selection range is now empty, then clear the selection.
				if (this.position == this.selectionStart)
				{
					this.ClearSelection();

					// Set the flag so we don't refire the SelectionChanged event below.
					clearSelection = true;
				}

				if (this.lines != null)
				{
					this.PositionChanged?.Invoke(this, EventArgs.Empty);

					// If we cleared the selection earlier, then that
					// fire a SelectionChanged event.  If not, then we
					// need to fire it now because we've changed the
					// selection end point.
					if (!clearSelection)
					{
						this.FireSelectionChanged();
					}
				}
			}

			if (scrollToCaret)
			{
				this.ScrollToCaret();
			}
		}

		private void SetSelectionEnd(int line, int column, bool scrollToCaret = true)
		{
			bool selectionChanged = false;
			if (!this.HasSelection)
			{
				this.selectionStart = this.position;
				selectionChanged = true;
			}

			// Move the Position but keep the selection start
			int originalLine = this.position.Line;
			this.SetPosition(line, column, false, scrollToCaret);
			int numLines = Math.Abs(line - originalLine);

			// Invalidate new selection
			int firstLine = Math.Min(originalLine, line);
			Point point = this.GetPointFromPos(firstLine, 0);
			Rectangle r = new(this.gutterWidth, point.Y, this.ClientSize.Width, (numLines + 1) * this.lineHeight);
			this.Invalidate(r);

			if (selectionChanged)
			{
				this.FireSelectionChanged();
			}
		}

		private void SetupScrollBars()
		{
			// Vertical - Scroll by lines
			int page = this.ClientSize.Height / this.lineHeight;
			int max = this.lines != null ? this.lines.Count - 1 : 0;
			NativeMethods.SetScrollPageAndRange(this, false, 0, max, page);

			// Horizontal - Scroll by characters
			page = this.ClientSize.Width / this.charWidth;
			max = 0;
			if (this.lines != null)
			{
				foreach (DiffViewLine line in this.lines)
				{
					int length = this.GetDisplayLine(line).GetDisplayTextLength();
					if (length > max)
					{
						max = length;
					}
				}
			}

			// We must include enough characters for the gutter line numbers and the separator.
			max += this.gutterWidth / this.charWidth;
			NativeMethods.SetScrollPageAndRange(this, true, 0, max, page);
		}

		private void UpdateAfterSetData()
		{
			// Reset the position before we start calculating things
			this.position = new DiffViewPosition(0, 0);
			this.selectionStart = DiffViewPosition.Empty;

			// We have to call this to recalc the gutter width
			this.UpdateTextMetrics(false);

			// We have to call this to setup the scroll bars
			this.SetupScrollBars();

			// Reset the scroll position
			this.VScrollPos = 0;
			this.HScrollPos = 0;

			// Update the caret
			this.UpdateCaret();

			// Force a repaint
			this.Invalidate();

			// Fire the LinesChanged event
			this.LinesChanged?.Invoke(this, EventArgs.Empty);

			// Fire the position changed event
			this.PositionChanged?.Invoke(this, EventArgs.Empty);

			this.FireSelectionChanged();
		}

		private void UpdateCaret()
		{
			if (this.lines != null && this.caret != null)
			{
				Point newPt = this.GetPointFromPos(this.position.Line, this.position.Column);
				this.caret.Visible = newPt.X >= (this.gutterWidth - GutterSeparatorWidth);
				this.caret.Position = newPt;
			}
		}

		private int UpdateScrollPos(int newPos, bool horz)
		{
			// Set the position and then retrieve it.  Due to adjustments by Windows
			// (e.g. if Pos is > Max) it may not be the same as the value set.
			NativeMethods.SetScrollPos(this, horz, newPos);
			return NativeMethods.GetScrollPos(this, horz);
		}

		private void UpdateTextMetrics(bool fontOrTabsChanged)
		{
			if (fontOrTabsChanged)
			{
				// Get the pixel width that a space should be.
				float dpi;
				using (Graphics g = Graphics.FromHwnd(this.Handle))
				{
					// See KBase article Q125681 for what I'm doing here to get the average character width.
					const string AvgCharWidthText = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
					this.charWidth = MeasureString(g, AvgCharWidthText, AvgCharWidthText.Length, this.Font) / AvgCharWidthText.Length;

					// Get the average pixels per inch
					dpi = (g.DpiX + g.DpiY) / 2;
				}

				// Get the line height in pixels
				FontFamily family = this.Font.FontFamily;
				int lineSpacingDesignUnits = family.GetLineSpacing(this.Font.Style);
				int fontHeightDesignUnits = family.GetEmHeight(this.Font.Style);
				float fontPoints = this.Font.Size;
				const int DefaultDpi = 72;
				float fontPixels = fontPoints * dpi / DefaultDpi;
				this.lineHeight = (int)Math.Ceiling((fontPixels * lineSpacingDesignUnits) / fontHeightDesignUnits);

				// This height still isn't "enough" (i.e. it still doesn't match
				// what the GetTextMetrics API would return as TEXTMETRICS.Height
				// + TEXTMETRICS.ExternalLeading.  It seems to be one pixel too
				// short, so I'll just add it back.
				this.lineHeight++;
			}

			// Set the gutter width to the this.CharWidth times the
			// number of characters we'll need to display.  Then
			// add another character for padding, another
			// pixel so we can have a separator line, and then
			// a small separator window-colored area.
			int maxLineNumChars = 1;
			if (this.LineCount > 0 && this.lines != null)
			{
				// Get the largest number.  Add 1 to it because we will
				// when we display it.  This is important when the number
				// is 9, but will be displayed as 10, etc.
				//
				// Also, we want to take the max of MaxLineNumber and 1 to
				// ensure that we never take the Log of 1 or less.  Negatives
				// and 0 don't have Logs, and Log(1) returns 0.  We always
				// want to end up with at least one for maxLineNumChars.
				int maxLineNumber = Math.Max(this.lines.MaxLineNumber, 1) + 1;

				// If the number of lines is NNNN (e.g. 1234), we need to get 4.
				// Add 1 and take the floor so that 10, 100, 1000, etc. will work
				// correctly.
				maxLineNumChars = (int)Math.Floor(Math.Log10(maxLineNumber) + 1);
			}

			this.gutterWidth = (this.charWidth * (maxLineNumChars + 1)) + 1 + GutterSeparatorWidth;

			// Build the gutter format string
			const int BufferSize = 20;
			StringBuilder sb = new(BufferSize);
			sb.Append("{0:");
			sb.Append('0', maxLineNumChars);
			sb.Append('}');
			this.gutterFormat = sb.ToString();

			// Update the caret position (Gutter width or Font changes affect it)
			this.UpdateCaret();
		}

		private void HandleArrowUpDown(int lines, bool ctrlPressed, bool shiftPressed, bool noModifierPressed)
		{
			if (ctrlPressed)
			{
				this.VScrollPos += lines;
				this.OffsetPosition(lines, 0);
			}
			else if (shiftPressed)
			{
				this.ExtendSelection(lines, 0);
			}
			else if (noModifierPressed)
			{
				this.OffsetPosition(lines, 0);
			}
		}

		private void HandleArrowLeft(bool ctrlShiftPressed, bool ctrlPressed, bool shiftPressed, bool noModifierPressed)
		{
			DiffViewPosition pos = this.Position;
			if (pos.Line > 0 && pos.Column <= 0)
			{
				// We're at the beginning of a line other than the first line,
				// so we need to move back to the end of the previous line.
				int prevLine = pos.Line - 1;
				DisplayLine prevDisplayLine = this.GetDisplayLine(prevLine);
				int prevLineLength = prevDisplayLine.GetDisplayTextLength();
				if (shiftPressed || ctrlShiftPressed)
				{
					this.SetSelectionEnd(prevLine, prevLineLength);
				}
				else if (noModifierPressed || ctrlPressed)
				{
					this.SetPosition(prevLine, prevLineLength);
				}
			}
			else
			{
				if (ctrlShiftPressed)
				{
					int startColumn = this.FindCurrentTokenStart(pos, false);
					this.SetSelectionEnd(pos.Line, startColumn);
				}
				else if (ctrlPressed)
				{
					int startColumn = this.FindCurrentTokenStart(pos, false);
					this.SetPosition(pos.Line, startColumn);
				}
				else if (shiftPressed)
				{
					this.ExtendSelection(0, -1);
				}
				else if (noModifierPressed)
				{
					this.OffsetPosition(0, -1);
				}
			}
		}

		private void HandleArrowRight(bool ctrlShiftPressed, bool ctrlPressed, bool shiftPressed, bool noModifierPressed)
		{
			DiffViewPosition pos = this.Position;
			DisplayLine displayLine = this.GetDisplayLine(pos.Line);
			int lineLength = displayLine.GetDisplayTextLength();
			if (pos.Line < (this.LineCount - 1) && pos.Column >= lineLength)
			{
				// We're at the end of a line other than the last line,
				// so we need to move to the beginning of the next line.
				if (shiftPressed || ctrlShiftPressed)
				{
					this.SetSelectionEnd(pos.Line + 1, 0);
				}
				else if (noModifierPressed || ctrlPressed)
				{
					this.SetPosition(pos.Line + 1, 0);
				}
			}
			else
			{
				if (ctrlShiftPressed)
				{
					int endColumn = this.FindCurrentTokenEnd(pos, false);
					this.SetSelectionEnd(pos.Line, endColumn);
				}
				else if (ctrlPressed)
				{
					int endColumn = this.FindCurrentTokenEnd(pos, true);
					this.SetPosition(pos.Line, endColumn);
				}
				else
				{
					// Tab characters can be 1 to N columns wide, so we need
					// to find out how many columns over we have to go.
					int offset = displayLine.GetColumnWidth(pos.Column);
					if (shiftPressed)
					{
						this.ExtendSelection(0, offset);
					}
					else if (noModifierPressed)
					{
						this.OffsetPosition(0, offset);
					}
				}
			}
		}

		private void HandlePageUpDown(int sign, bool shiftPressed, bool noModifierPressed)
		{
			int page = NativeMethods.GetScrollPage(this, false);
			if (shiftPressed)
			{
				this.ExtendSelection(sign * page, 0);
			}
			else if (noModifierPressed)
			{
				this.VScrollPos += sign * page;
				this.OffsetPosition(sign * page, 0);
			}
		}

		#endregion
	}
}