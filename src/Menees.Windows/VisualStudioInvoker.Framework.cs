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

		// .NET Framework fully supports the dynamic keyword for COM Interop.
		private static object EnsureDynamic(object value) => value;

		#endregion
	}
}
