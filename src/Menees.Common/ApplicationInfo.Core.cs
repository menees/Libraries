namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Text;

	#endregion

	public static partial class ApplicationInfo
	{
		#region Public Properties

		/// <summary>
		/// Gets whether the current user is running in the "Administrator" (i.e., sudo) role.
		/// </summary>
		public static bool IsUserRunningAsAdministrator
		{
			get
			{
				bool result;

				// https://stackoverflow.com/a/52745016/1882616
				if (IsWindows)
				{
					result = IsWindowsUserRunningAsAdministrator;
				}
				else
				{
					result = NativeMethods.GetUid() != 0;
				}

				return result;
			}
		}

		/// <summary>
		/// Gets whether the current application is running on a Windows OS.
		/// </summary>
		public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		#endregion

	}
}
