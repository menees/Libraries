namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Configuration;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime;
	using System.Security.Principal;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Menees.Diagnostics;

	#endregion

	/// <summary>
	/// Provides information about the current application.
	/// </summary>
	public static partial class ApplicationInfo
	{
		#region Private Data Members

		private static readonly Lazy<int> LazyProcessId = new(() =>
			{
				using (Process current = Process.GetCurrentProcess())
				{
					return current.Id;
				}
			});

		private static string? applicationName;
		private static int showingUnhandledExceptionErrorMessage;
		private static bool? isDebugBuild;
		private static Func<bool>? isActivated;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the application name.
		/// </summary>
		public static string ApplicationName
		{
			get
			{
				string? result = applicationName;

				if (string.IsNullOrEmpty(result))
				{
					result = GlobalLogContext.GetDefaultApplicationName();
				}

				return result!;
			}
		}

		/// <summary>
		/// Gets the base directory for the current application.
		/// </summary>
		/// <remarks>
		/// This is usually the same as the <see cref="ExecutableFile"/>'s directory, but it can be
		/// different for applications using custom AppDomains (e.g., web apps running in IIS).
		/// </remarks>
		public static string BaseDirectory
		{
			get
			{
				string result = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
				return result;
			}
		}

		/// <summary>
		/// Gets the full path for the executable file that started the application.
		/// </summary>
		/// <remarks>
		/// This is similar to System.Windows.Forms.Application.ExecutablePath, except this supports paths
		/// longer than MAX_PATH (260) and paths using a "\\?\" prefix (e.g., ASP.NET worker processes).
		/// </remarks>
		public static string ExecutableFile
		{
			get
			{
				string result = NativeMethods.GetModuleFileName(IntPtr.Zero);
				return result;
			}
		}

		/// <summary>
		/// Gets the current application's Windows process ID.
		/// </summary>
		public static int ProcessId => LazyProcessId.Value;

		/// <summary>
		/// Gets whether the current application is activated per the lambda passed to <see cref="Initialize"/>.
		/// </summary>
		public static bool IsActivated
		{
			get
			{
				bool result = isActivated?.Invoke() ?? false;
				return result;
			}
		}

		/// <summary>
		/// Gets whether the current application is running a debug build.
		/// </summary>
		/// <remarks>
		/// This depends on the applicationAssembly parameter passed to <see cref="Initialize"/>.
		/// If <see cref="Initialize"/>, hasn't been called, then this depends on the current assembly.
		/// </remarks>
		public static bool IsDebugBuild => isDebugBuild ?? IsDebugAssembly(typeof(ApplicationInfo).Assembly);

		#endregion

		#region Private Properties

		private static bool IsWindowsUserRunningAsAdministrator
		{
			get
			{
				Conditions.RequireState(IsWindows, "This can only be called safely on Windows.");

#pragma warning disable CA1416 // Validate platform compatibility. The callers in .Core.cs and .Framework.cs ensure Windows.
				using (WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent())
				{
					WindowsPrincipal currentPrincipal = new(currentIdentity);
					bool result = currentPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
					return result;
				}
#pragma warning restore CA1416 // Validate platform compatibility
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Used to initialize the application's name, error handling, etc.
		/// </summary>
		/// <param name="applicationName">Pass null to use the current AppDomain's friendly name.</param>
		/// <param name="applicationAssembly">The assembly that's initializing the application, typically the main executable.</param>
		/// <param name="isActivated">A function to determine if <see cref="IsActivated"/> should consider the application activated.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Initialize(string applicationName, Assembly? applicationAssembly = null, Func<bool>? isActivated = null)
		{
			// Try to log when unhandled or unobserved exceptions occur.  This info can be very useful if the process crashes.
			// Note: Windows Forms unhandled exceptions are logged via Menees.Windows.Forms.WindowsUtility.InitializeApplication.
			AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			{
				LogLevel level = e.IsTerminating ? LogLevel.Fatal : LogLevel.Error;
				Log.Write(typeof(ApplicationInfo), level, "An unhandled exception occurred.", e.ExceptionObject as Exception);
			};

			TaskScheduler.UnobservedTaskException += (s, e) =>
			{
				Log.Error(typeof(ApplicationInfo), "A Task exception occurred, but it was never observed by the Task caller.", e.Exception);
			};

			// If the name is null or empty, then the property accessor will use the AppDomain's friendly name.
			ApplicationInfo.applicationName = applicationName;
			ApplicationInfo.isActivated = isActivated;

			// Put it in the log's global context, so it will appear in every log entry.
			if (!string.IsNullOrEmpty(applicationName))
			{
				Log.GlobalContext.SetApplicationName(applicationName);
			}

			// Since apps refer to this library via NuGet references, they'll always use the release build of this library.
			// So we'll check the main assembly's build configuration via reflection instead of using a compile-time constant.
			Assembly assembly = applicationAssembly ?? Assembly.GetEntryAssembly() ?? typeof(ApplicationInfo).Assembly;
			isDebugBuild = IsDebugAssembly(assembly);

			// Call SetErrorMode to disable the display of Windows Shell modal error dialogs for
			// file not found, Windows Error Reporting, and other errors.  From SetErrorMode docs
			// at http://msdn.microsoft.com/en-us/library/ms680621.aspx:
			// 		"Best practice is that all applications call the process-wide SetErrorMode
			// 		function with a parameter of SEM_FAILCRITICALERRORS at startup. This is
			// 		to prevent error mode dialogs from hanging the application."
			NativeMethods.DisableShellModalErrorDialogs();

			InitializeTargetFramework();
		}

		/// <summary>
		/// Creates a hierarchical store for loading and saving user-level settings for the current application.
		/// </summary>
		/// <returns>A new settings store.</returns>
		public static ISettingsStore CreateUserSettingsStore() => new FileSettingsStore();

		/// <summary>
		/// Shows a single unhandled exception at a time.
		/// </summary>
		/// <param name="ex">The exception to show</param>
		/// <param name="showExceptionMessage">The action to invoke for a simple "MessageBox" display.</param>
		/// <param name="showExceptionCustom">The action to invoke for a custom exception display.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void ShowUnhandledException(
			Exception ex,
			Action<string>? showExceptionMessage,
			Action<Exception>? showExceptionCustom = null)
		{
			// Only let the first thread in show a message box.
			int showing = Interlocked.Increment(ref showingUnhandledExceptionErrorMessage);
			try
			{
				if (showing == 1)
				{
					if (showExceptionCustom != null)
					{
						showExceptionCustom(ex);
					}
					else if (showExceptionMessage != null)
					{
						StringBuilder sb = new();
						sb.AppendLine(Exceptions.GetMessage(ex));
						if (IsDebugBuild)
						{
							// Show the root exception's type and call stack in debug builds.
							sb.AppendLine().AppendLine(ex.ToString());
						}

						string message = sb.ToString().Trim();
						showExceptionMessage(message);
					}
				}
			}
			finally
			{
				Interlocked.Decrement(ref showingUnhandledExceptionErrorMessage);
			}
		}

		#endregion

		#region Private Methods

		static partial void InitializeTargetFramework();

		private static bool IsDebugAssembly(Assembly assembly)
		{
			bool result = false;

			if (assembly != null)
			{
				var configuration = (AssemblyConfigurationAttribute?)assembly.GetCustomAttribute(typeof(AssemblyConfigurationAttribute));
				result = configuration?.Configuration?.Contains("Debug") ?? false;
			}

			return result;
		}

		#endregion
	}
}
