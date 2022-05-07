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
	/// NOTE: Using this requires multiple steps. First, the calling program's App.xaml has to merge in this assembly's
	/// resources like:
	/// <code>
	/// <Application.Resources>
	/// 	<ResourceDictionary>
	/// 		<ResourceDictionary.MergedDictionaries>
	/// 			<ResourceDictionary Source="pack://application:,,,/Menees.Windows.Presentation;component/SharedResources.xaml" />
	/// 		</ResourceDictionary.MergedDictionaries>
	/// 	</ResourceDictionary>
	/// </Application.Resources>
	/// </code>
	/// Second, any control assigning to the "PlaceholderText attached property also needs to explicitly assign an
	/// appropriate Style to display the placeholder. The Style doesn't default by TargetType because I didn't want to make
	/// all TextBoxBase and PasswordBox controls suddenly have a PlaceholderText when they didn't before. This way it's opt-in.
	/// The relevant styles are:
	/// <list type="bullet">
	/// <item>Style="{StaticResource Menees.Windows.Presentation.PlaceholderTextBox}"</item>
	/// <item>Style="{StaticResource Menees.Windows.Presentation.PlaceholderPasswordBox}"</item>
	/// </list>
	/// <para/>
	/// This class is based on https://prabu-guru.blogspot.com/2010/06/how-to-add-watermark-text-to-textbox.html.
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

		private static readonly List<(Type DependencyObjectType, Action<DependencyObject, bool> OnIsMonitoringChanged)> CustomChangeMonitors = new();

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

		#region Public Methods

		/// <summary>
		/// Makes the <see cref="IsMonitoringProperty"/> extensible for custom types that need placeholder text.
		/// </summary>
		/// <typeparam name="T">The type of object to invoke custom monitoring for.</typeparam>
		/// <param name="onIsMonitoringChanged">The callback to invoke when "IsMonitoring" has changed.
		/// The bool value represents the current "IsMonitoring" state. The callback should attach or detach
		/// from the custom control's "TextChanged" event (or something similar), and that event handler
		/// should call this type's <see cref="SetHasText"/> method to indicate when the control has text.
		/// </param>
		/// <remarks>
		/// Any control type using this will also need to create its own Style resource similar to
		/// "Menees.Windows.Presentation.PlaceholderTextBox", and it can use the
		/// "Menees.Windows.Presentation.PlaceholderText.ControlTemplate" resource if the
		/// control has a "PART_ContentHost" named part that displays the content of the element.
		/// </remarks>
		public static void AddCustomChangeMonitor<T>(Action<T, bool> onIsMonitoringChanged)
			where T : DependencyObject
		{
			CustomChangeMonitors.Add((typeof(T), (d, value) => onIsMonitoringChanged((T)d, value)));
		}

		#endregion

		#region Private Methods

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
			else
			{
				CustomChangeMonitors.Where(tuple => tuple.DependencyObjectType.IsInstanceOfType(d))
					.Select(tuple => tuple.OnIsMonitoringChanged)
					.FirstOrDefault()
					?.Invoke(d, (bool)e.NewValue);
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
