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

	/// <summary>
	/// Single pane that a diff utility would display on the left or right.
	/// </summary>
	internal sealed partial class DiffView : Control
	{
		#region Constructors

		[SuppressMessage(
			"Microsoft.Mobility",
			"CA1601:DoNotUseTimersThatPreventPowerStateChanges",
			Justification = "This timer is only used to auto-scroll while the mouse is captured and dragging.")]
		public DiffView()
		{
			// Set some important control styles
			this.SetStyle(ControlStyles.Opaque, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.DoubleBuffer, true);
			this.SetStyle(ControlStyles.Selectable, true);
			this.SetStyle(ControlStyles.StandardClick, true);
			this.SetStyle(ControlStyles.StandardDoubleClick, true);
			this.SetStyle(ControlStyles.ResizeRedraw, true);

			this.position = new DiffViewPosition(0, 0);

			this.UpdateTextMetrics(true);

			this.autoScrollTimer = new Timer();
			this.autoScrollTimer.Enabled = false;
			this.autoScrollTimer.Interval = 100;
			this.autoScrollTimer.Tick += this.AutoScrollTimer_Tick;

			DiffOptions.OptionsChanged += this.DiffOptionsChanged;

			this.Cursor = Cursors.IBeam;
		}

		#endregion

		#region Public Events

		public event EventHandler HScrollPosChanged;

		public event EventHandler LinesChanged;

		public event EventHandler PositionChanged;

		public event EventHandler SelectionChanged;

		public event EventHandler VScrollPosChanged;

		#endregion

		#region Public Properties

		[Browsable(false)]
		public bool CanGoToFirstDiff
		{
			get
			{
				bool result = false;

				if (this.lines != null)
				{
					int[] starts = this.lines.DiffStartLines;
					int[] ends = this.lines.DiffEndLines;
					result = starts.Length > 0 && ends.Length > 0 && (this.position.Line < starts[0] || this.position.Line > ends[0]);
				}

				return result;
			}
		}

		[Browsable(false)]
		public bool CanGoToLastDiff
		{
			get
			{
				bool result = false;

				if (this.lines != null)
				{
					int[] starts = this.lines.DiffStartLines;
					int[] ends = this.lines.DiffEndLines;
					result = starts.Length > 0 && ends.Length > 0 &&
						(this.position.Line < starts[starts.Length - 1] || this.position.Line > ends[ends.Length - 1]);
				}

				return result;
			}
		}

		[Browsable(false)]
		public bool CanGoToNextDiff
		{
			get
			{
				bool result = false;
				if (this.lines != null)
				{
					int[] starts = this.lines.DiffStartLines;
					result = starts.Length > 0 && this.position.Line < starts[starts.Length - 1];
				}

				return result;
			}
		}

		[Browsable(false)]
		public bool CanGoToPreviousDiff
		{
			get
			{
				bool result = false;

				if (this.lines != null)
				{
					int[] ends = this.lines.DiffEndLines;
					result = ends.Length > 0 && this.position.Line > ends[0];
				}

				return result;
			}
		}

		[Browsable(false)]
		[SuppressMessage(
			"Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "The get_CenterVisibleLine accessor is only called by the Windows Forms designer via reflection.")]
		public int CenterVisibleLine
		{
			get
			{
				int result = this.FirstVisibleLine + (this.VisibleLineCount / 2);
				return result;
			}

			set
			{
				// Make this line the center of the view.
				int firstLine = value - (this.VisibleLineCount / 2);
				this.FirstVisibleLine = firstLine;
			}
		}

		public ChangeDiffOptions ChangeDiffOptions
		{
			get;
			set;
		}

		[Browsable(false)]
		public int FirstVisibleLine
		{
			get
			{
				return this.VScrollPos;
			}

			set
			{
				this.VScrollPos = value;
			}
		}

		public bool HasSelection => this.selectionStart != DiffViewPosition.Empty;

		[Browsable(false)]
		public int HScrollPos
		{
			get
			{
				return NativeMethods.GetScrollPos(this, true);
			}

			set
			{
				this.ScrollHorizontally(value, this.HScrollPos);
			}
		}

		[Browsable(false)]
		public int LineCount => this.lines != null ? this.lines.Count : 0;

		/// <summary>
		/// Stores each line's text, color, and original number.
		/// </summary>
		[Browsable(false)]
		public DiffViewLines Lines => this.lines;

		[Browsable(false)]
		public DiffViewPosition Position
		{
			get
			{
				return this.position;
			}

			set
			{
				if (!this.position.Equals(value))
				{
					this.SetPosition(value.Line, value.Column, true, false);
				}
			}
		}

		public string SelectedText
		{
			get
			{
				string result = string.Empty;

				if (this.HasSelection && this.lines != null)
				{
					this.GetForwardOrderSelection(out DiffViewPosition startSel, out DiffViewPosition endSel);

					int numLines = endSel.Line - startSel.Line + 1;
					const int LineLengthEstimate = 50;
					StringBuilder sb = new StringBuilder(numLines * LineLengthEstimate);

					for (int i = startSel.Line; i <= endSel.Line; i++)
					{
						// Leave out lines that are only in the display for alignment
						// purposes.  This makes SelectedText useful for "Compare Text",
						// and typically much more useful for "Copy".
						DiffViewLine line = this.lines[i];
						if (line.Number.HasValue)
						{
							DisplayLine displayLine = this.GetDisplayLine(line);
							int displayLength = displayLine.GetDisplayTextLength();
							int selStartColumn = (i == startSel.Line) ? startSel.Column : 0;
							int selEndColumn = (i == endSel.Line) ? Math.Min(endSel.Column, displayLength) : displayLength;

							bool lineFullySelected = (i > startSel.Line && i < endSel.Line) || (selStartColumn == 0 && selEndColumn == displayLength);
							string originalText;
							if (lineFullySelected)
							{
								originalText = line.Text;
							}
							else
							{
								originalText = displayLine.GetTextBetweenDisplayColumns(selStartColumn, selEndColumn);
							}

							sb.Append(originalText);
							if (i != endSel.Line)
							{
								sb.AppendLine();
							}
						}
					}

					result = sb.ToString();
				}

				return result;
			}
		}

		[DefaultValue(false)]
		public bool ShowWhitespace
		{
			get
			{
				return this.showWhitespace;
			}

			set
			{
				if (this.showWhitespace != value)
				{
					this.showWhitespace = value;
					this.Invalidate();
				}
			}
		}

		[Browsable(false)]
		public int VisibleLineCount => this.ClientSize.Height / this.lineHeight;

		[Browsable(false)]
		public int VScrollPos
		{
			get
			{
				return NativeMethods.GetScrollPos(this, false);
			}

			set
			{
				this.ScrollVertically(value, this.VScrollPos);
			}
		}

		#endregion

		#region Protected Properties

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams result = base.CreateParams;
				result.Style = result.Style | NativeMethods.WS_VSCROLL | NativeMethods.WS_HSCROLL;
				NativeMethods.SetBorderStyle(result, this.borderStyle);
				return result;
			}
		}

		#endregion

		#region Public Methods

		public bool Find(FindData data)
		{
			// If text is selected on a single line, then use that for the new Find text.
			string originalFindText = data.Text;
			if (this.GetSingleLineSelectedText(out string selectedText))
			{
				data.Text = selectedText;
			}

			bool result = false;
			using (FindDialog dialog = new FindDialog())
			{
				if (dialog.Execute(this, data))
				{
					if (data.SearchUp)
					{
						result = this.FindPrevious(data);
					}
					else
					{
						result = this.FindNext(data);
					}
				}
				else
				{
					// Reset the Find text if the user cancelled.
					data.Text = originalFindText;
				}
			}

			return result;
		}

		public bool FindNext(FindData data)
		{
			bool result = false;

			if (string.IsNullOrEmpty(data.Text))
			{
				data.SearchUp = false;
				result = this.Find(data);
			}
			else
			{
				int numLines = this.LineCount;
				if (numLines > 0)
				{
					string text = data.Text;
					if (!data.MatchCase)
					{
						text = text.ToUpper();
					}

					DiffViewPosition startPosition = this.GetFindStartPosition(false);
					int lastLineLastColumn = startPosition.Column;

					// Use <= so we check the start line again from the beginning
					for (int i = 0; i <= numLines; i++)
					{
						// Use % so we wrap around at the end.
						int lineNumber = (startPosition.Line + i) % numLines;

						// This needs to search the original text.
						DisplayLine displayLine = this.GetDisplayLine(lineNumber);
						string line = displayLine.OriginalText;

						if (!data.MatchCase)
						{
							line = line.ToUpper();
						}

						int index;
						if (i == numLines)
						{
							// We're rechecking the start line from the beginning.
							index = line.IndexOf(text, 0, displayLine.GetTextIndexFromDisplayColumn(lastLineLastColumn));
						}
						else
						{
							index = line.IndexOf(text, displayLine.GetTextIndexFromDisplayColumn(startPosition.Column));
						}

						if (index >= 0)
						{
							this.GoToPosition(lineNumber, displayLine.GetDisplayColumnFromTextIndex(index));
							this.ExtendSelection(0, text.Length);
							result = true;
							break;
						}

						// On all lines but the first, we need to start at 0
						startPosition = new DiffViewPosition(startPosition.Line, 0);
					}
				}

				if (!result)
				{
					string message = string.Format("'{0}' was not found.", data.Text);
					MessageBox.Show(this, message, nameof(this.Find), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}

			return result;
		}

		public bool FindPrevious(FindData data)
		{
			bool result = false;

			if (string.IsNullOrEmpty(data.Text))
			{
				data.SearchUp = true;
				result = this.Find(data);
			}
			else
			{
				int numLines = this.LineCount;
				if (numLines > 0)
				{
					string text = data.Text;
					if (!data.MatchCase)
					{
						text = text.ToUpper();
					}

					DiffViewPosition startPosition = this.GetFindStartPosition(true);
					int lastLineLastColumn = startPosition.Column;

					// Use <= so we check the start line again from the end
					for (int i = 0; i <= numLines; i++)
					{
						// Use % so we wrap around at the end.
						int lineNumber = (startPosition.Line - i + numLines) % numLines;

						// This needs to search the original text.
						DisplayLine displayLine = this.GetDisplayLine(lineNumber);
						string line = displayLine.OriginalText;

						if (!data.MatchCase)
						{
							line = line.ToUpper();
						}

						const int StartAtEndColumn = -1;
						if (startPosition.Column == StartAtEndColumn)
						{
							startPosition = new DiffViewPosition(startPosition.Line, Math.Max(0, displayLine.GetDisplayTextLength()));
						}

						int index;
						if (i == numLines)
						{
							// We're rechecking the start line from the end.
							int startIndex = displayLine.GetTextIndexFromDisplayColumn(startPosition.Column);
							int lastIndex = displayLine.GetTextIndexFromDisplayColumn(lastLineLastColumn);
							index = line.LastIndexOf(text, startIndex, startIndex - lastIndex + 1);
						}
						else
						{
							index = line.LastIndexOf(text, displayLine.GetTextIndexFromDisplayColumn(startPosition.Column));
						}

						if (index >= 0)
						{
							this.GoToPosition(lineNumber, displayLine.GetDisplayColumnFromTextIndex(index));
							this.ExtendSelection(0, text.Length);
							result = true;
							break;
						}

						// On all lines but the first, we need to start at the end
						startPosition = new DiffViewPosition(startPosition.Line, StartAtEndColumn);
					}
				}

				if (!result)
				{
					string message = string.Format("'{0}' was not found.", data.Text);
					MessageBox.Show(this, message, nameof(this.Find), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}

			return result;
		}

		public Point GetPointFromPos(int line, int column)
		{
			int y = (line - this.VScrollPos) * this.lineHeight;

			// Because we're not guaranteed to have a monospaced font,
			// this gets tricky.  We have to measure the substring to
			// get the correct X.
			DisplayLine displayLine = this.GetDisplayLine(line);
			using (Graphics g = Graphics.FromHwnd(this.Handle))
			{
				int x = this.GetXForColumn(g, displayLine, null, column);
				return new Point(x, y);
			}
		}

		public DiffViewPosition GetPosFromPoint(int x, int y)
		{
			int line = (y / this.lineHeight) + this.VScrollPos;

			// Because we're not guaranteed to have a monospaced font,
			// this gets tricky.  We have to make an initial guess at
			// the column, and then we'll converge to the best one.
			DisplayLine displayLine = this.GetDisplayLine(line);
			string text = displayLine.GetDisplayText();

			// Make a starting guess.  Because of tabs and variable width
			// fonts, this may be nowhere near the right place...
			int column = (int)((x - this.gutterWidth + (this.HScrollPos * this.charWidth)) / this.charWidth);

			using (Graphics g = Graphics.FromHwnd(this.Handle))
			{
				int textLength = text.Length;
				int columnGreater = -1;
				int columnLess = -1;

				int columnX = this.GetXForColumn(g, displayLine, text, column);
				if (columnX != x)
				{
					if (columnX > x)
					{
						columnGreater = column;
						columnLess = 0;
						for (column = columnGreater - 1; column >= 0; column--)
						{
							columnX = this.GetXForColumn(g, displayLine, text, column);
							if (columnX > x)
							{
								columnGreater = column;
							}
							else
							{
								columnLess = column;
								break;
							}
						}
					}
					else
					{
						// columnX < X
						columnLess = column;
						columnGreater = textLength;
						for (column = columnLess + 1; column <= textLength; column++)
						{
							columnX = this.GetXForColumn(g, displayLine, text, column);
							if (columnX < x)
							{
								columnLess = column;
							}
							else
							{
								columnGreater = column;
								break;
							}
						}
					}

					columnGreater = EnsureInRange(0, columnGreater, textLength);
					columnLess = EnsureInRange(0, columnLess, textLength);

					int greaterX = this.GetXForColumn(g, displayLine, text, columnGreater);
					int lessX = this.GetXForColumn(g, displayLine, text, columnLess);

					if (Math.Abs(greaterX - x) < Math.Abs(lessX - x))
					{
						column = columnGreater;
					}
					else
					{
						column = columnLess;
					}
				}
			}

			return new DiffViewPosition(line, column);
		}

		public bool GoToFirstDiff()
		{
			bool result = false;

			if (this.CanGoToFirstDiff)
			{
				this.GoToPosition(this.lines.DiffStartLines[0], this.position.Column);
				result = true;
			}

			return result;
		}

		public bool GoToLastDiff()
		{
			bool result = false;

			if (this.CanGoToLastDiff)
			{
				int[] starts = this.lines.DiffStartLines;
				this.GoToPosition(starts[starts.Length - 1], this.position.Column);
				result = true;
			}

			return result;
		}

		public bool GoToLine()
		{
			int maxLineNumber = 0;
			for (int i = this.lines.Count - 1; i >= 0; i--)
			{
				DiffViewLine line = this.lines[i];
				if (line.Number.HasValue)
				{
					// Add 1 because display numbers are 1-based.
					maxLineNumber = line.Number.Value + 1;
					break;
				}
			}

			bool result = false;
			if (maxLineNumber > 0)
			{
				using (GoToDialog dialog = new GoToDialog())
				{
					if (dialog.Execute(this, maxLineNumber, out int line))
					{
						// Subtract 1 because the dialog returns a 1-based number
						result = this.GoToLine(line - 1);
					}
				}
			}

			return result;
		}

		public bool GoToLine(int line)
		{
			bool result = false;

			// We know the original line number will be in a DiffViewLine at a position >= iLine.
			if (line >= 0 && this.lines != null && line < this.lines.Count)
			{
				for (int i = line; i < this.lines.Count; i++)
				{
					DiffViewLine diffLine = this.lines[i];
					if (diffLine.Number.HasValue && diffLine.Number.Value == line)
					{
						this.GoToPosition(i, 0);
						result = true;
					}
				}
			}

			return result;
		}

		public bool GoToNextDiff()
		{
			bool result = false;

			if (this.CanGoToNextDiff)
			{
				int[] starts = this.lines.DiffStartLines;
				int numStarts = starts.Length;
				for (int i = 0; i < numStarts; i++)
				{
					if (this.position.Line < starts[i])
					{
						this.GoToPosition(starts[i], this.position.Column);
						result = true;
						break;
					}
				}
			}

			return result;
		}

		public bool GoToPreviousDiff()
		{
			bool result = false;

			if (this.CanGoToPreviousDiff)
			{
				int[] ends = this.lines.DiffEndLines;
				int numEnds = ends.Length;
				for (int i = numEnds - 1; i >= 0; i--)
				{
					if (this.position.Line > ends[i])
					{
						// I'm intentionally setting the line to Starts[i] here instead of Ends[i].
						this.GoToPosition(this.lines.DiffStartLines[i], this.position.Column);
						result = true;
						break;
					}
				}
			}

			return result;
		}

		public void ScrollToCaret()
		{
			if (this.caret != null)
			{
				// Assume the caret is always at this.Position.
				// It would be nice if we had:
				// Debug.Assert(this.Position.Line == CaretPos.Line && this.Position.Column == CaretPos.Column);
				// but that fails on occasion because of rounding problems
				// between calling GetPointFromPos and then GetPosFromPoint.
				DiffViewPosition caretPos = this.position;
				Point caretPoint = this.GetPointFromPos(caretPos.Line, caretPos.Column);

				// Make sure that position is on the screen by
				// scrolling the minimal number of lines and characters.
				int firstVisibleLine = this.FirstVisibleLine;
				int lastVisibleLine = firstVisibleLine + this.VisibleLineCount - 1;

				if (caretPos.Line < firstVisibleLine)
				{
					this.VScrollPos -= firstVisibleLine - caretPos.Line;
				}
				else if (caretPos.Line > lastVisibleLine)
				{
					this.VScrollPos += caretPos.Line - lastVisibleLine;
				}

				// This is tricky because we might not have a monospaced font.
				// We have to figure out the number of pixels we need to scroll
				// and then translate that into characters (i.e. CharWidths).
				int firstVisibleX = this.gutterWidth - GutterSeparatorWidth;
				int lastVisibleX = this.ClientSize.Width - this.caret.Size.Width;
				if (caretPoint.X < firstVisibleX)
				{
					int scrollPixels = caretPoint.X - firstVisibleX;
					this.HScrollPos += (int)Math.Floor(scrollPixels / (double)this.charWidth);
				}
				else if (caretPoint.X > lastVisibleX)
				{
					int scrollPixels = caretPoint.X - lastVisibleX;
					this.HScrollPos += (int)Math.Ceiling(scrollPixels / (double)this.charWidth);
				}
			}
		}

		public void SelectAll()
		{
			this.SetPosition(0, 0, true, false);
			DiffViewPosition endPos = this.GetDocumentEndPosition();
			this.SetSelectionEnd(endPos.Line, endPos.Column, false);
		}

		public void SetCounterpartLines(DiffView counterpartView)
		{
			int numLines = this.LineCount;
			if (numLines != counterpartView.LineCount)
			{
				throw new ArgumentException("The counterpart view has a different number of view lines.", nameof(counterpartView));
			}

			for (int i = 0; i < numLines; i++)
			{
				DiffViewLine line = this.lines[i];
				DiffViewLine counterpart = counterpartView.lines[i];

				// Make the counterpart lines refer to each other.
				line.Counterpart = counterpart;
				counterpart.Counterpart = line;
			}
		}

		public void SetData(IList<string> stringList, EditScript script, bool useA)
		{
			this.lines = new DiffViewLines(stringList, script, useA);
			this.UpdateAfterSetData();
		}

		public void SetData(DiffViewLine lineOne, DiffViewLine lineTwo)
		{
			this.lines = new DiffViewLines(lineOne, lineTwo);
			this.UpdateAfterSetData();
		}

		#endregion

		#region Internal Methods

		internal static int EnsureInRange(int min, int value, int max)
		{
			int result = Math.Max(min, Math.Min(value, max));
			return result;
		}

		#endregion

		#region Protected Methods

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.caret?.Dispose();
				this.autoScrollTimer?.Dispose();

				DiffOptions.OptionsChanged -= this.DiffOptionsChanged;
			}

			base.Dispose(disposing);
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			// Select the identifier at the double-clicked position.
			DiffViewPosition pos = this.Position;
			int startColumn = this.FindCurrentTokenStart(pos, true);
			int endColumn = this.FindCurrentTokenEnd(pos, false);
			this.SetPosition(pos.Line, startColumn);
			if (endColumn != startColumn)
			{
				this.SetSelectionEnd(pos.Line, endColumn);
			}

			base.OnDoubleClick(e);
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			this.Invalidate();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			// Do these first so event handlers can pull correct values.
			this.UpdateTextMetrics(true);
			this.SetupScrollBars();

			// Now, call the base handler and let it notify registered delegates.
			base.OnFontChanged(e);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			this.ReleaseCaret();
			this.caret = new Caret(this, CaretWidth, this.lineHeight);
			this.UpdateCaret();

			this.InvalidateSelection();
			this.InvalidateCaretGutter();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			bool ctrlShiftPressed = e.Modifiers == (Keys.Control | Keys.Shift);
			bool ctrlPressed = e.Modifiers == Keys.Control;
			bool shiftPressed = e.Modifiers == Keys.Shift;
			bool noModifierPressed = e.Modifiers == 0;

			if (ctrlShiftPressed || ctrlPressed || shiftPressed || noModifierPressed)
			{
				switch (e.KeyCode)
				{
					case Keys.A:
						if (ctrlPressed)
						{
							this.SelectAll();
						}

						break;

					case Keys.C:
						if (ctrlPressed && this.HasSelection)
						{
							Clipboard.SetDataObject(this.SelectedText, true);
						}

						break;

					case Keys.Up:
						this.HandleArrowUpDown(-1, ctrlPressed, shiftPressed, noModifierPressed);
						break;

					case Keys.Down:
						this.HandleArrowUpDown(+1, ctrlPressed, shiftPressed, noModifierPressed);
						break;

					case Keys.Left:
						this.HandleArrowLeft(ctrlShiftPressed, ctrlPressed, shiftPressed, noModifierPressed);
						break;

					case Keys.Right:
						this.HandleArrowRight(ctrlShiftPressed, ctrlPressed, shiftPressed, noModifierPressed);
						break;

					case Keys.PageUp:
						this.HandlePageUpDown(-1, shiftPressed, noModifierPressed);
						break;

					case Keys.PageDown:
						this.HandlePageUpDown(+1, shiftPressed, noModifierPressed);
						break;

					case Keys.Home:
						if (ctrlShiftPressed)
						{
							this.SetSelectionEnd(0, 0);
						}
						else if (ctrlPressed)
						{
							this.SetPosition(0, 0);
						}
						else if (shiftPressed)
						{
							this.ExtendSelection(0, -this.Position.Column);
						}
						else if (noModifierPressed)
						{
							this.SetPosition(this.Position.Line, 0);
						}

						break;

					case Keys.End:
						if (ctrlShiftPressed)
						{
							DiffViewPosition endPos = this.GetDocumentEndPosition();
							this.SetSelectionEnd(endPos.Line, endPos.Column);
						}
						else if (ctrlPressed)
						{
							DiffViewPosition endPos = this.GetDocumentEndPosition();
							this.SetPosition(endPos.Line, endPos.Column);
						}
						else if (shiftPressed)
						{
							this.ExtendSelection(0, this.GetDisplayLineLength(this.Position.Line) - this.Position.Column);
						}
						else if (noModifierPressed)
						{
							int line = this.Position.Line;
							this.SetPosition(line, this.GetDisplayLineLength(line));
						}

						break;
				}
			}
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);

			this.ReleaseCaret();

			this.InvalidateSelection();
			this.InvalidateCaretGutter();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (!this.Focused && this.CanFocus)
			{
				this.Focus();
			}

			if (e.X >= this.gutterWidth)
			{
				DiffViewPosition pos = this.GetPosFromPoint(e.X, e.Y);

				// Only change pos if non-right-click or right-click not in selection
				if (e.Button != MouseButtons.Right || !this.InSelection(pos))
				{
					this.SetPosition(pos.Line, pos.Column);
				}

				if (e.Button == MouseButtons.Left)
				{
					this.Capture = true;
					this.capturedMouse = true;
				}
			}

			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			// Update the mouse cursor to be an IBeam if we're not in the gutter.
			this.Cursor = (e.X < this.gutterWidth) ? Cursors.Default : Cursors.IBeam;

			if (this.capturedMouse && e.Button == MouseButtons.Left)
			{
				// Determine if we're at or above the first visible line
				// or at or below the last visible line.  If so, then
				// auto-scroll.  Similarly, if we're on the first or last
				// character or beyond, then auto-scroll.
				Rectangle r = new Rectangle(this.gutterWidth, 0, this.ClientSize.Width, this.ClientSize.Height);
				r.Inflate(-this.charWidth, -this.lineHeight);
				if (!r.Contains(e.X, e.Y))
				{
					this.verticalAutoScrollAmount = 0;
					if (e.Y < r.Y)
					{
						this.verticalAutoScrollAmount = -1;
					}
					else if (e.Y > r.Bottom)
					{
						this.verticalAutoScrollAmount = 1;
					}

					this.horizontalAutoScrollAmount = 0;
					if (e.X < r.X)
					{
						this.horizontalAutoScrollAmount = -1;
					}
					else if (e.X > r.Right)
					{
						this.horizontalAutoScrollAmount = 1;
					}

					this.autoScrollTimer.Enabled = true;
				}
				else
				{
					this.autoScrollTimer.Enabled = false;
				}

				// Set the selection end to the current mouse position
				// if the new position is different from the caret position.
				DiffViewPosition pos = this.GetPosFromPoint(e.X, e.Y);
				if (pos != this.position)
				{
					this.SetSelectionEnd(pos.Line, pos.Column);
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (this.capturedMouse && e.Button == MouseButtons.Left)
			{
				this.Capture = false;
				this.capturedMouse = false;
				this.autoScrollTimer.Enabled = false;
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			int windowsWheelDelta = SystemInformation.MouseWheelScrollDelta;
			this.wheelDelta += e.Delta;
			if (Math.Abs(this.wheelDelta) >= windowsWheelDelta && windowsWheelDelta > 0)
			{
				// I'm using "-=" here because Delta is reversed from what seems normal to me.
				// (e.g. wheel scrolling towards the user returns a negative value).
				this.VScrollPos -= SystemInformation.MouseWheelScrollLines * (this.wheelDelta / windowsWheelDelta);
				this.wheelDelta = 0;
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			// Get scroll positions
			int posY = this.VScrollPos;
			int posX = this.HScrollPos;

			// Find painting limits
			int numLines = this.LineCount;
			int firstLine = Math.Max(0, posY + (e.ClipRectangle.Top / this.lineHeight));
			int lastCalcLine = posY + (e.ClipRectangle.Bottom / this.lineHeight);
			int lastLine = Math.Min(numLines - 1, lastCalcLine);

			// Create some graphics objects
			Graphics g = e.Graphics;
			using (SolidBrush fontBrush = new SolidBrush(this.Enabled ? this.ForeColor : SystemColors.GrayText))
			using (SolidBrush backBrush = new SolidBrush(this.BackColor))
			{
				// We can't free GutterBrush since it is a system brush.
				Brush gutterBrush = SystemBrushes.Control;

				// Set the correct origin for HatchBrushes (used when painting dead space).
				g.RenderingOrigin = new Point(-posX, -posY);

				// See what we need to paint.  For horz scrolling,
				// the gutter won't need it.  For focus changes,
				// the lines won't need it.
				bool paintGutter = e.ClipRectangle.X < this.gutterWidth;
				bool paintLine = e.ClipRectangle.X + e.ClipRectangle.Width >= this.gutterWidth;
				bool hasFocus = this.Focused;

				// Indent the gutter text horizontally a little bit
				int lineNumIndent = this.charWidth / 2; // This centers it since it has 1 extra char width

				// Determine the selection positions in forward order
				bool hasSelection = this.HasSelection;
				this.GetForwardOrderSelection(out DiffViewPosition startSel, out DiffViewPosition endSel);

				// Paint each line
				for (int i = firstLine; i <= lastLine; i++)
				{
					// If we get inside this loop there must be at least one line.
					Debug.Assert(this.LineCount > 0, "There must be at least one line.");

					int x = (this.charWidth * (-posX)) + this.gutterWidth;
					int y = this.lineHeight * (i - posY);

					DiffViewLine line = this.lines[i];
					if (paintLine)
					{
						this.DrawLine(g, fontBrush, backBrush, hasFocus, i, line, x, y, hasSelection, startSel, endSel);
					}

					if (paintGutter)
					{
						this.DrawGutter(g, fontBrush, backBrush, hasFocus, i, line, y, gutterBrush, lineNumIndent);
					}
				}

				// Draw the background and an empty gutter for any
				// blank lines past the end of the actual lines.
				backBrush.Color = this.BackColor;
				for (int i = lastLine + 1; i <= lastCalcLine; i++)
				{
					int y = this.lineHeight * (i - posY);

					this.DrawBackground(g, backBrush, y, true);

					if (paintGutter)
					{
						this.DrawGutterBackground(g, gutterBrush, backBrush, y);
					}
				}
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			// We must setup the scroll bars before calling the base handler.
			// Attached event handlers like DiffOverview need to be able to pull
			// the correct FirstVisibleLine and VisibleLineCount properties.
			this.SetupScrollBars();

			// Now, call the base handler and let it notify registered delegates.
			base.OnSizeChanged(e);
		}

		protected override bool IsInputKey(Keys keyData)
		{
			bool result;

			// We have to do this so the arrow keys will go to our OnKeyDown method.
			// Alternately, we could handle WM_GETDLGCODE in WndProc and include
			// the DLGC_WANTARROWS flag, but this way is easier and fully managed.
			if (keyData.HasFlag(Keys.Left) ||
				keyData.HasFlag(Keys.Right) ||
				keyData.HasFlag(Keys.Up) ||
				keyData.HasFlag(Keys.Down))
			{
				result = true;
			}
			else
			{
				result = base.IsInputKey(keyData);
			}

			return result;
		}

		protected override void WndProc(ref System.Windows.Forms.Message m)
		{
			if (m.Msg == NativeMethods.WM_VSCROLL || m.Msg == NativeMethods.WM_HSCROLL)
			{
				bool horz = m.Msg == NativeMethods.WM_HSCROLL;

				NativeMethods.ScrollInfo info = NativeMethods.GetScrollInfo(this, horz);
				int newPos = info.nPos;
				int originalPos = newPos;

				// The SB_THUMBTRACK code is only in the lower word.
				const int ThumbTrackMask = 0xFFFF;
				ushort scrollCode = (ushort)((int)m.WParam & ThumbTrackMask);
				switch (scrollCode)
				{
					case NativeMethods.SB_TOP: // SB_LEFT
						newPos = info.nMin;
						break;

					case NativeMethods.SB_BOTTOM: // SB_RIGHT
						newPos = info.nMax;
						break;

					case NativeMethods.SB_LINEUP: // SB_LINELEFT;
						newPos--;
						break;

					case NativeMethods.SB_LINEDOWN: // SB_LINERIGHT
						newPos++;
						break;

					case NativeMethods.SB_PAGEUP: // SB_PAGELEFT
						newPos -= (int)info.nPage;
						break;

					case NativeMethods.SB_PAGEDOWN: // SB_PAGERIGHT
						newPos += (int)info.nPage;
						break;

					case NativeMethods.SB_THUMBTRACK:
						newPos = info.nTrackPos;
						break;
				}

				if (horz)
				{
					this.ScrollHorizontally(newPos, originalPos);
				}
				else
				{
					this.ScrollVertically(newPos, originalPos);
				}
			}
			else
			{
				base.WndProc(ref m);
			}
		}

		#endregion
	}
}
