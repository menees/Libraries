namespace Menees.Windows
{
	#region Using Directives

	using System;
	using System.Runtime.InteropServices;

	#endregion

	public static partial class ComUtility
	{
		#region Internal Methods

		internal static object GetActiveObject(string progId) => Marshal.GetActiveObject(progId);

		#endregion
	}
}
