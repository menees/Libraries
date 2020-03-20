namespace Menees.Windows
{
	#region Using Directives

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
		public static int FinalRelease(object instance)
		{
			if (instance is ComObject comObject)
			{
				instance = comObject.Instance;
			}

			int result = Marshal.FinalReleaseComObject(instance);
			return result;
		}

		#endregion

		#region Internal Methods

		internal static object GetActiveObject(string progID) => NativeMethods.GetActiveObject(progID);

		internal static object EnsureDynamic(object value)
		{
			// .NET Core 3.x doesn't support dynamic for COM Interop.
			// https://github.com/dotnet/runtime/issues/30502#issuecomment-518748077
			// To get around that limitation until .NET 5, we have to use a DynamicObject.
			// https://github.com/dotnet/runtime/issues/12587#issuecomment-585591984
			// https://github.com/dotnet/runtime/issues/12587#issuecomment-534611966
			dynamic result = new ComObject(value);
			return result;
		}

		#endregion
	}
}
