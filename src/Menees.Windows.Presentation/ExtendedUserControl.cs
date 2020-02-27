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
	/// The base class for Menees WPF UserControls.
	/// </summary>
	public partial class ExtendedUserControl : UserControl
	{
		#region Constructors

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public ExtendedUserControl()
		{
			// Note: Xaml classes can't derive from Xaml classes, so we have to do this in code.
			// http://support.microsoft.com/kb/957231
			//
			// Make the text look MUCH better on screen.
			// http://blogs.msdn.com/b/text/archive/2009/08/24/wpf-4-0-text-stack-improvements.aspx
			TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
		}

		#endregion
	}
}
