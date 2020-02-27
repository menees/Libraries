namespace Menees.Windows.Forms
{
	#region Using Directives

	using System.Runtime.InteropServices;

	#endregion

	public static partial class VisualStudioInvoker
	{
		#region Private Methods

		private static object GetActiveObject(string progID)
		{
			// TODO: We can't call Marshal.GetActiveObject(progID) in .NET Core. [Bill, 2/26/2020]
			// Find some alternative like P/Invoking COM's GetActiveObject as mentioned in Marshal.GetActiveObject's help.
			string.IsNullOrEmpty(progID);
			return null;
		}

		#endregion
	}
}
