namespace Menees.Windows
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading;
	using Microsoft.CSharp.RuntimeBinder;
	using Microsoft.VisualStudio.Setup.Configuration;

	#endregion

	/// <summary>
	/// Used to open files in Visual Studio and locate its executables.
	/// </summary>
	public static partial class VisualStudioUtility
	{
		#region Public Constants

		/// <summary>
		/// VS 2017 is version 15, and it includes MSBuild v15.
		/// </summary>
		public const int VS2017MajorVersion = 15;

		#endregion

		#region Public Methods

		/// <summary>
		/// Opens a file in Visual Studio.
		/// </summary>
		/// <param name="fileName">The full path to a file to open.</param>
		/// <param name="fileLineNumber">The 1-based line number to go to in the file.</param>
		/// <returns>True if it was successful.  False if the file couldn't be opened in Visual Studio.</returns>
		/// <remarks>
		/// Using WindowsUtility.ShellExecute, it's very easy to open a file that's associated
		/// with Visual Studio.  But to force a file open in Visual Studio and then
		/// jump to a specific line number is a lot more work.
		/// <para/>
		/// The bulk of the code for this class was shamelessly pulled from FxCop 1.35's
		/// Microsoft.FxCop.UI.FxCopUI class (in FxCop.exe) using Reflector and the
		/// FileDisassembler add-in.  Then it was updated to use C# 4's dynamic keyword
		/// to make the run-time invocation easier. Then I had to muck with it even more
		/// to make it work in .NET Core 3.1.
		/// </remarks>
		public static bool OpenFile(string fileName, string fileLineNumber)
		{
			bool result = false;

			try
			{
				// Use late-bound COM to open the file in Visual Studio
				// so we can jump to a specific line number.  This also
				// allows us to reuse an open instance of VS.
				//
				// We could execute Visual Studio by command-line and
				// run the GotoLn command (like MegaBuild does), but that
				// requires starting a new instance of VS for each file opened.
				dynamic? dte = GetVisualStudioInstance();
				if (dte != null)
				{
					OpenInVisualStudio(dte, fileName, fileLineNumber);
					result = true;
				}
			}
#pragma warning disable CC0004 // Catch block cannot be empty
			catch (COMException)
			{
				// Treat any COM exception as "can't open" (i.e., false result).
			}
			catch (ArgumentException)
			{
				// Treat any argument exception as "can't open" (i.e., false result).
			}
#pragma warning restore CC0004 // Catch block cannot be empty

			return result;
		}

		/// <summary>
		/// Uses the VS SetupConfiguration COM API to resolve a path to a version-specific executable like DevEnv.exe or MSBuild.exe.
		/// </summary>
		/// <param name="buildVersionPath">Used to build a version-specific path to try to resolve.</param>
		/// <param name="minMajorVersion">The minimum major version to look for. This defaults to <see cref="VS2017MajorVersion"/>.</param>
		/// <param name="maxMajorVersion">The maximum major version to look for. This defaults to <see cref="int.MaxValue"/>.</param>
		/// <param name="resolvedPathMustExist">Whether the resolved version-specific path must exist. This defaults to false.</param>
		/// <returns>The resolved path for the highest matched version or null if no version was matched.</returns>
		public static string? ResolvePath(
			Func<Version, string> buildVersionPath,
			int minMajorVersion = VS2017MajorVersion,
			int maxMajorVersion = int.MaxValue,
			bool resolvedPathMustExist = false)
		{
			string? result = null;
			Version? resultVersion = null;

			// VS 2017 and up allow multiple side-by-side editions to be installed, and we have to use a COM API to enumerate the installed instances.
			// https://github.com/mluparu/vs-setup-samples - COM API samples
			// https://code.msdn.microsoft.com/Visual-Studio-Setup-0cedd331 - More Q&A about the COM API samples
			// https://blogs.msdn.microsoft.com/vcblog/2017/03/06/finding-the-visual-c-compiler-tools-in-visual-studio-2017/#comment-273625
			// https://github.com/Microsoft/vswhere - A redistributable .exe for enumerating the VS instances from the command line.
			// https://blogs.msdn.microsoft.com/heaths/2016/09/15/changes-to-visual-studio-15-setup/
			const int REGDB_E_CLASSNOTREG = -2147221164; // 0x80040154
			try
			{
				// From MS example: https://github.com/Microsoft/vs-setup-samples/blob/master/Setup.Configuration.CS/Program.cs
				SetupConfiguration configuration = new();

				IEnumSetupInstances instanceEnumerator = configuration.EnumAllInstances();
				int fetched;
				ISetupInstance[] instances = new ISetupInstance[1];
				do
				{
					instanceEnumerator.Next(1, instances, out fetched);
					if (fetched > 0)
					{
						ISetupInstance instance = instances[0];
						if (instance != null
							&& Version.TryParse(instance.GetInstallationVersion(), out Version? version)
							&& version.Major >= minMajorVersion
							&& version.Major <= maxMajorVersion)
						{
							InstanceState state = ((ISetupInstance2)instance).GetState();
							if (state == InstanceState.Complete)
							{
								string? versionPath = buildVersionPath?.Invoke(version);
								if (!string.IsNullOrEmpty(versionPath))
								{
									string resolvedPath = instance.ResolvePath(versionPath);
									if ((resultVersion == null || resultVersion < version)
										&& (!resolvedPathMustExist || File.Exists(resolvedPath) || Directory.Exists(resolvedPath)))
									{
										result = resolvedPath;
										resultVersion = version;

										// If we're looking for a single version, then we don't need to keep looking.
										if (minMajorVersion == maxMajorVersion)
										{
											break;
										}
									}
								}
							}
						}
					}
				}
				while (fetched > 0);
			}
#pragma warning disable CC0004 // Catch block cannot be empty
			catch (COMException ex) when (ex.HResult == REGDB_E_CLASSNOTREG)
			{
				// The SetupConfiguration API is not registered, so assume no instances are installed.
			}
			catch (Exception)
			{
				// Heath Stewart (MSFT), the author of the SetupConfiguration API, says to treat any exception as "no instances installed."
				// https://code.msdn.microsoft.com/windowsdesktop/Visual-Studio-Setup-0cedd331/view/Discussions#content
			}
#pragma warning restore CC0004 // Catch block cannot be empty

			return result;
		}

		#endregion

		#region Private Methods

		private static object? GetVisualStudioInstance()
		{
			object? result = null;

			const string VisualStudioProgId = "VisualStudio.DTE";
			try
			{
				// See if there's a running instance of VS.
				result = ComUtility.GetActiveObject(VisualStudioProgId);
			}
			catch (COMException)
			{
				// See if VS is registered.
				Type? dteType = Type.GetTypeFromProgID(VisualStudioProgId, false);
				if (dteType != null)
				{
					// VS is registered, so we need to start a new instance.
					result = Activator.CreateInstance(dteType);
				}
			}

			result = ComUtility.EnsureDynamic(result);
			return result;
		}

		private static void OpenInVisualStudio(dynamic dte, string fileName, string line)
		{
			dte.ExecuteCommand("File.OpenFile", TextUtility.EnsureQuotes(fileName));
			dte.ExecuteCommand("Edit.Goto", line);

			dynamic mainWindow = dte.MainWindow;
			object hWnd = mainWindow.HWnd;
			if (hWnd is IntPtr mainWindowHandle)
			{
				NativeMethods.BringWindowForward(mainWindowHandle);
				const int MillisecondsPerSecond = 1000;
				Thread.Sleep(MillisecondsPerSecond);

				mainWindow.Activate();
				mainWindow.Visible = true;
			}
		}

		#endregion
	}
}
