namespace Menees.Windows
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
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
		public static object CreateInstance(string progId)
		{
			Type t = Type.GetTypeFromProgID(progId);
			object instance = Activator.CreateInstance(t);
			return EnsureDynamic(instance);
		}

		#endregion
	}
}
