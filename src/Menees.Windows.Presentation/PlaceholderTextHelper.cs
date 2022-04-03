namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Documents;
	using System.Windows.Input;

	#endregion

	/// <summary>
	/// Adds "PlaceholderText" and "HasText" attached properties to <see cref="TextBoxBase"/> and <see cref="PasswordBox"/>.
	/// </summary>
	/// <remarks>
	/// Based on https://prabu-guru.blogspot.com/2010/06/how-to-add-watermark-text-to-textbox.html.
	/// For a more complex implementation that was also based on the same starter example see:
	/// https://github.com/MahApps/MahApps.Metro/blob/develop/src/MahApps.Metro/Controls/Helper/TextBoxHelper.cs
	/// </remarks>
	public sealed class PlaceholderTextHelper : DependencyObject
	{
		#region Public Fields For Attached Properties

		/// <summary>
		/// Gets the dependency property for the "internal" IsMonitoring field.
		/// </summary>
		/// <remarks>
		/// This shouldn't be used directly. This is used internally by style resource setters.
		/// </remarks>
		public static readonly DependencyProperty IsMonitoringProperty =
			DependencyProperty.RegisterAttached(
				"IsMonitoring",
				typeof(bool),
				typeof(PlaceholderTextHelper),
				new UIPropertyMetadata(false, OnIsMonitoringChanged));

		/// <summary>
		/// Gets the dependency property for the PlaceholderText field.
		/// </summary>
		public static readonly DependencyProperty PlaceholderTextProperty =
			DependencyProperty.RegisterAttached(
				"PlaceholderText",
				typeof(string),
				typeof(PlaceholderTextHelper),
				new UIPropertyMetadata(string.Empty));

		/// <summary>
		/// Gets the dependency property for the HasText field.
		/// </summary>
		public static readonly DependencyProperty HasTextProperty =
			DependencyProperty.RegisterAttached(
				"HasText",
				typeof(bool),
				typeof(PlaceholderTextHelper),
				new FrameworkPropertyMetadata(false, AffectsDisplay));

		#endregion

		#region Private Data Members

		private const FrameworkPropertyMetadataOptions AffectsDisplay = FrameworkPropertyMetadataOptions.AffectsMeasure
			| FrameworkPropertyMetadataOptions.AffectsArrange
			| FrameworkPropertyMetadataOptions.AffectsRender;

		#endregion

		#region Public Attached Properties

		/// <summary>
		/// Gets whether placeholder text is being monitored.
		/// </summary>
		/// <remarks>
		/// This is used internally by style resource setters.
		/// </remarks>
		public static bool GetIsMonitoring(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsMonitoringProperty);
		}

		/// <summary>
		/// Sets whether placeholder text is being monitored.
		/// </summary>
		/// <remarks>
		/// This is used internally by style resource setters.
		/// </remarks>
		public static void SetIsMonitoring(DependencyObject obj, bool value)
		{
			obj.SetValue(IsMonitoringProperty, value);
		}

		/// <summary>
		/// Gets the placeholder text for the specified object.
		/// </summary>
		[AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
		[AttachedPropertyBrowsableForType(typeof(PasswordBox))]
		public static string GetPlaceholderText(DependencyObject obj)
		{
			return (string)obj.GetValue(PlaceholderTextProperty);
		}

		/// <summary>
		/// Sets the placeholder text for the specified object.
		/// </summary>
		[AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
		[AttachedPropertyBrowsableForType(typeof(PasswordBox))]
		public static void SetPlaceholderText(DependencyObject obj, string value)
		{
			obj.SetValue(PlaceholderTextProperty, value);
		}

		/// <summary>
		/// Gets if the attached text box has text.
		/// </summary>
		[AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
		[AttachedPropertyBrowsableForType(typeof(PasswordBox))]
		public static bool GetHasText(DependencyObject obj)
		{
			return (bool)obj.GetValue(HasTextProperty);
		}

		/// <summary>
		/// Sets if the attached text box has text.
		/// </summary>
		[AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
		[AttachedPropertyBrowsableForType(typeof(PasswordBox))]
		public static void SetHasText(DependencyObject obj, bool value)
		{
			obj.SetValue(HasTextProperty, value);
		}

		#endregion

		#region Implementation

		private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is TextBoxBase txtBox)
			{
				if ((bool)e.NewValue)
				{
					txtBox.TextChanged += TextChanged;
				}
				else
				{
					txtBox.TextChanged -= TextChanged;
				}
			}
			else if (d is PasswordBox passBox)
			{
				if ((bool)e.NewValue)
				{
					passBox.PasswordChanged += PasswordChanged;
				}
				else
				{
					passBox.PasswordChanged -= PasswordChanged;
				}
			}
		}

		private static void TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender is TextBox txtBox)
			{
				SetHasText(txtBox, txtBox.Text.Length > 0);
			}
			else if (sender is RichTextBox richTextBox)
			{
				// This gets the text length of the first line. RichTextBox always adds a final NewLine.
				// From https://github.com/MahApps/MahApps.Metro/blob/develop/src/MahApps.Metro/Controls/Helper/TextBoxHelper.cs
				var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
				var text = textRange.Text;
				var lastIndexOfNewLine = text.LastIndexOf(Environment.NewLine, StringComparison.InvariantCulture);
				if (lastIndexOfNewLine >= 0)
				{
					text = text.Remove(lastIndexOfNewLine);
				}

				SetHasText(richTextBox, text.Length > 0);
			}
		}

		private static void PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (sender is PasswordBox passBox)
			{
				SetHasText(passBox, passBox.Password.Length > 0);
			}
		}

		#endregion
	}
}
