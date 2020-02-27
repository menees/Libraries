namespace Menees.Windows.Forms
{
	#region Using Directives

	using System.Runtime.InteropServices;

	#endregion

	public static partial class VisualStudioInvoker
	{
		#region Private Methods

		private static object GetActiveObject(string progID) => Marshal.GetActiveObject(progID);

		#endregion
	}
}
