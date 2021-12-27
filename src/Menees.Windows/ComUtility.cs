namespace Menees.Windows
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Text;

	#endregion

	/// <summary>
	/// Helper methods for working with COM objects via dynamic dispatch.
	/// </summary>
	public static partial class ComUtility
	{
		#region Public Methods

		/// <summary>
		/// Creates a new instance of an object via its ProgId and ensures it
		/// is accessible via C#'s dynamic keyword.
		/// </summary>
		/// <param name="progId">The ProgId of the type to create.</param>
		/// <returns>A new dynamic instance.</returns>
		public static object? CreateInstance(string progId)
		{
			Type? type = Type.GetTypeFromProgID(progId);
			object? instance = type != null ? Activator.CreateInstance(type) : null;
			return EnsureDynamic(instance);
		}

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

		// .NET Framework and .NET 5.0 fully support the dynamic keyword for COM Interop.
		// https://github.com/dotnet/runtime/issues/12587
		internal static object? EnsureDynamic(object? value) => value;

		#endregion
	}
}
