namespace Menees.Windows
{
	#region Using Directives

	using System;
	using System.Runtime.InteropServices;

	#endregion

	public static partial class ComUtility
	{
		#region Public Methods

		/// <summary>
		/// Performs the final release on a COM object's runtime callable wrapper (RCW).
		/// </summary>
		/// <param name="instance">The instance to release.</param>
		/// <returns>
		/// The new value of the reference count of the RCW associated with <paramref name="instance"/>,
		/// which is 0 (zero) if the release is successful.
		/// </returns>
		public static int FinalRelease(object instance) => Marshal.FinalReleaseComObject(instance);

		#endregion

		#region Internal Methods

		internal static object GetActiveObject(string progID) => Marshal.GetActiveObject(progID);

		// .NET Framework fully supports the dynamic keyword for COM Interop.
		internal static object EnsureDynamic(object value) => value;

		#endregion
	}
}
