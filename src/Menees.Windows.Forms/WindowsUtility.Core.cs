namespace Menees.Windows.Forms
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Windows.Forms;

	#endregion

	public static partial class WindowsUtility
	{
		#region Private Methods

		static partial void SetHighDpiMode()
		{
			// DpiUnawareGdiScaled works best for my old WinForms apps (e.g., MegaBuild), which are designed at 96 dpi.
			// This method returns a bool, and its result will be false if an earlier call sets the HighDpiMode to something else.
			// That call just needs to be made from Program.cs before calling WindowsUtility.InitializeApplication.
			Application.SetHighDpiMode(HighDpiMode.DpiUnawareGdiScaled);
		}

		#endregion
	}
}
