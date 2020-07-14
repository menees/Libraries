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
	/// Methods for working with windows and dialogs using an IntPtr window handle.
	/// </summary>
	/// <remarks>
	/// This provides low-level APIs that need to be shared with Menees.Windows.Forms
	/// and Menees.Windows.Presentation.
	/// </remarks>
	public static class HandleUtility
	{
		#region Public Methods

		/// <summary>
		/// Sets the show state of a window without waiting for the operation to complete by
		/// posting a ShowWindow command to the message queue of the given window.
		/// </summary>
		/// <param name="hWnd">The handle of the window to post to.</param>
		/// <param name="command">The command to post.</param>
		/// <returns>True if the command was posted; false otherwise.</returns>
		public static bool PostShowWindowCommand(IntPtr hWnd, ShowWindowCommand command)
			=> NativeMethods.PostShowWindowCommand(hWnd, command);

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
