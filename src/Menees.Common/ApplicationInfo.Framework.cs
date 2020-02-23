namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime;
	using System.Security.Principal;
	using System.Text;
	using System.Threading.Tasks;

	#endregion

	public static partial class ApplicationInfo
	{
		#region Public Properties

		/// <summary>
		/// Gets whether the current user is running in the Windows "Administrator" role.
		/// </summary>
		public static bool IsUserRunningAsAdministrator
		{
			get
			{
				using (WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent())
				{
					WindowsPrincipal currentPrincipal = new WindowsPrincipal(currentIdentity);
					bool result = currentPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
					return result;
				}
			}
		}

		#endregion

		#region Private Methods

		static partial void InitializeTargetFramework()
		{
			// Enable profile-guided optimizations (PGO or "Pogo") if we're not in unit tests, web apps, etc.
			// http://blogs.msdn.com/b/dotnet/archive/2012/10/18/an-easy-solution-for-improving-app-launch-performance.aspx
			if (AppDomain.CurrentDomain.IsDefaultAppDomain())
			{
				ProfileOptimization.SetProfileRoot(Path.GetTempPath());
				ProfileOptimization.StartProfile(ApplicationName + ".pgo");
			}
		}

		#endregion
	}
}
