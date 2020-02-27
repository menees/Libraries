namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;

	#endregion

	/// <summary>
	/// The base class for Menees WPF dialog windows.
	/// </summary>
	public class ExtendedDialog : ExtendedWindow
	{
		#region Public Fields

		/// <summary>
		/// The DialogResult dependency property.
		/// </summary>
		[SuppressMessage("", "SA1118", Justification = "Dependency property initialization is best with parameters defined inline.")]
		public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached(
			"DialogResult",
			typeof(bool?),
			typeof(ExtendedDialog),
			new UIPropertyMetadata
			{
				// Idea came from: http://stackoverflow.com/questions/1759372/where-is-button-dialogresult-in-wpf/1759505#1759505
				PropertyChangedCallback = (obj, e) =>
				{
					if (!(obj is Button button))
					{
						throw Exceptions.NewInvalidOperationException("DialogResult can only be used on a Button control.");
					}

					button.Click += (sender, e2) =>
					{
						Window.GetWindow(button).DialogResult = GetDialogResult(button);
					};
				},
			});

		/// <summary>
		/// The ShowIcon dependency property.
		/// </summary>
		[SuppressMessage("", "SA1118", Justification = "Dependency property initialization is best with parameters defined inline.")]
		public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
			"ShowIcon",
			typeof(bool),
			typeof(ExtendedDialog),
			new FrameworkPropertyMetadata(
				false,
				new PropertyChangedCallback((obj, e) =>
					{
						if (!(obj is ExtendedDialog dialog))
						{
							throw Exceptions.NewInvalidOperationException("ShowIcon can only be used on an ExtendedDialog.");
						}
					})));

		#endregion

		#region Constructors

		[SuppressMessage(
			"Microsoft.Performance",
			"CA1810:InitializeReferenceTypeStaticFieldsInline",
			Justification = "Changing WPF's static property metadata requires a static constructor.")]
		static ExtendedDialog()
		{
			// Tell WPF that we're changing some default property values from their Window settings to better values for modal dialogs.
			Window.ResizeModeProperty.OverrideMetadata(typeof(ExtendedDialog), new FrameworkPropertyMetadata(ResizeMode.NoResize));
			Window.ShowInTaskbarProperty.OverrideMetadata(typeof(ExtendedDialog), new FrameworkPropertyMetadata(false));
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public ExtendedDialog()
		{
			// WindowStartupLocation isn't a dependency property, so have to change its default from the instance constructor.
			this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the DialogResult dependency property value for the specified button.
		/// </summary>
		[SuppressMessage(
			"Microsoft.Design",
			"CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "The PropertyChangedCallback requires a Button.")]
#pragma warning disable CA1721 // Property names should not match get methods
		public static bool? GetDialogResult(Button button) => (bool?)button.GetValue(DialogResultProperty);
#pragma warning restore CA1721 // Property names should not match get methods

		/// <summary>
		/// Sets the DialogResult dependency property value for the specified button.
		/// </summary>
		[SuppressMessage(
			"Microsoft.Design",
			"CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "The PropertyChangedCallback requires a Button.")]
		public static void SetDialogResult(Button button, bool? value)
		{
			button.SetValue(DialogResultProperty, value);
		}

		/// <summary>
		/// Gets the ShowIcon dependency property value for the dialog.
		/// </summary>
		[SuppressMessage(
			"Microsoft.Design",
			"CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "The PropertyChangedCallback requires an ExtendedDialog.")]
		public static bool GetShowIcon(ExtendedDialog dialog) => (bool)dialog.GetValue(ShowIconProperty);

		/// <summary>
		/// Sets the ShowIcon dependency property value for the dialog.
		/// </summary>
		[SuppressMessage(
			"Microsoft.Design",
			"CA1011:ConsiderPassingBaseTypesAsParameters",
			Justification = "The PropertyChangedCallback requires an ExtendedDialog.")]
		public static void SetShowIcon(ExtendedDialog dialog, bool value)
		{
			dialog.SetValue(ShowIconProperty, value);
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Applies the ShowIcon dependency property.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnSourceInitialized(EventArgs e)
		{
			// Idea(s) came from:
			// http://stackoverflow.com/questions/3096359/wpf-remove-system-menu-icon-from-modal-window-but-not-main-app-window?rq=1
			// http://stackoverflow.com/questions/18580430/hide-the-icon-from-a-wpf-window
			// http://stackoverflow.com/questions/2341230/removing-icon-from-a-wpf-window
			// http://www.wpftutorial.net/RemoveIcon.html
			// WPF will show the icon by default, so we're ok as long as this property isn't toggled at run-time.
			if (!GetShowIcon(this))
			{
				NativeMethods.RemoveIcon(this);
			}

			base.OnSourceInitialized(e);
		}

		#endregion
	}
}
