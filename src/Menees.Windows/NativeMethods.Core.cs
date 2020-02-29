namespace Menees.Windows
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Text;

	#endregion

	internal static partial class NativeMethods
	{
		#region Public Methods

		public static object GetActiveObject(string progId)
		{
			// This code started from ILSpy's disassembly of .NET Framework's Marshal.GetActiveObject.
			// But I changed it to use PreserveSig = true for CLSIDFromProgIDEx since the original code
			// just caught any exception and deferred to CLSIDFromProgID.
			if (CLSIDFromProgIDEx(progId, out Guid guid) != 0)
			{
				CLSIDFromProgID(progId, out guid);
			}

			GetActiveObject(ref guid, IntPtr.Zero, out object result);
			return result;
		}

		#endregion

		#region Private Methods

		[DllImport("ole32.dll", PreserveSig = false)]
		private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

		[DllImport("ole32.dll", PreserveSig = true)]
		private static extern int CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

		[DllImport("oleaut32.dll", PreserveSig = false)]
		private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);

		#endregion
	}
}
