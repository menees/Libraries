namespace Menees.Windows
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Text;

	#endregion

	/// <summary>
	/// Methods for working with the Windows shell.
	/// </summary>
	public static class DialogUtility
	{
		#region Public Methods

		/// <summary>
		/// Selects a file system path and allows the user to type in a path if necessary.
		/// </summary>
		/// <param name="ownerHandle">The handle of the owner of the displayed modal dialog.</param>
		/// <param name="title">A short description of the path being selected.</param>
		/// <param name="initialFolder">The initial path to select.</param>
		/// <returns>The path the user selected if they pressed OK.  Null otherwise (e.g., the user canceled).</returns>
		public static string SelectFolder(IntPtr? ownerHandle, string title, string initialFolder)
			=> NativeMethods.SelectFolder(ownerHandle, title, initialFolder);

		#endregion
	}
}
