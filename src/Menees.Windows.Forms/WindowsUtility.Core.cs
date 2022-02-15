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
			Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
		}

		#endregion
	}
}
