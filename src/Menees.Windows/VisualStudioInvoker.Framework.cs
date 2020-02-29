namespace Menees.Windows
{
	#region Using Directives

	using System;
	using System.Runtime.InteropServices;

	#endregion

	public static partial class VisualStudioInvoker
	{
		#region Private Methods

		private static object GetActiveObject(string progID) => Marshal.GetActiveObject(progID);

		private static void ExecuteCommand(dynamic dte, string command, string arg) => dte.ExecuteCommand(command, arg);

		private static IntPtr GetMainWindowHandle(dynamic dte)
		{
			dynamic mainWindow = dte.MainWindow;
			IntPtr result = (IntPtr)Convert.ToInt64(mainWindow.HWnd);
			return result;
		}

		private static void ActivateMainWindow(dynamic dte)
		{
			dynamic mainWindow = dte.MainWindow;
			mainWindow.Activate();
			mainWindow.Visible = true;
		}

		#endregion
	}
}
