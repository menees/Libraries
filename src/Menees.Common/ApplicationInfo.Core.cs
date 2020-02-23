namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Text;

	#endregion

	public static partial class ApplicationInfo
	{
		#region Public Properties

		/// <summary>
		/// Gets whether the current user is running in the Windows "Administrator" role.
		/// </summary>
		// TODO: Finish IsUserRunningAsAdministrator. https://stackoverflow.com/a/52745016. [Bill, 1/1/2020]
		public static bool IsUserRunningAsAdministrator => false;

		#endregion
	}
}
