namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;

	#endregion

	/// <summary>
	/// A button that looks like a hyperlink.
	/// </summary>
	public sealed class HyperlinkButton : Button
	{
		#region Constructors

		static HyperlinkButton()
		{
			// Tell the framework where to find our default style.
			// http://stackoverflow.com/questions/1729161/wpf-custom-derived-control-style
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(HyperlinkButton),
				new FrameworkPropertyMetadata(typeof(HyperlinkButton)));
		}

		#endregion
	}
}
