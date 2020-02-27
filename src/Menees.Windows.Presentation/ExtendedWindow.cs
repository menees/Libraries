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
	using System.Windows.Media;

	#endregion

	/// <summary>
	/// The base class for Menees WPF Windows.
	/// </summary>
	public partial class ExtendedWindow : Window
	{
		#region Constructors

		static ExtendedWindow()
		{
			// Tell WPF that we're changing the default TextFormattingMode to Display for this type.
			TextOptions.TextFormattingModeProperty.OverrideMetadata(
				typeof(ExtendedWindow),
				new FrameworkPropertyMetadata(TextFormattingMode.Display));
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public ExtendedWindow()
		{
			// Note: Xaml classes can't derive from Xaml classes, so we have to do this in code.
			// http://support.microsoft.com/kb/957231
			//
			// Make the text look MUCH better on screen.
			// http://blogs.msdn.com/b/text/archive/2009/08/24/wpf-4-0-text-stack-improvements.aspx
			//
			// Note 2: I don't really understand why this call is necessary.  I expected the OverrideMetadata
			// call to be sufficient, but it doesn't seem to have the desired effect on the attached property.
			TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);

			// Try to make controls align to pixel boundaries too.
			this.UseLayoutRounding = true;
		}

		#endregion
	}
}
