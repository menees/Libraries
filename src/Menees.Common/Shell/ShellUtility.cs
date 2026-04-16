namespace Menees.Shell
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Versioning;
	using System.Text;

	#endregion

	/// <summary>
	/// Methods for working with the OS shell.
	/// </summary>
	public static class ShellUtility
	{
		#region Public Methods

		/// <summary>
		/// Gets the assembly's copyright in a displayable format (e.g., for an About box).
		/// </summary>
		/// <param name="assembly">The assembly to get the copyright from.</param>
		/// <returns>User-friendly copyright information.</returns>
		/// <remarks>
		/// If a copyright isn't found in the passed-in assembly or if it is null or empty,
		/// then the copyright information from the current assembly will be returned.
		/// </remarks>
		public static string? GetCopyrightInfo(Assembly? assembly)
		{
			string? result = null;

			if (assembly != null)
			{
				result = ReflectionUtility.GetCopyright(assembly);
			}

			if (string.IsNullOrEmpty(result))
			{
				Assembly current = Assembly.GetExecutingAssembly();
				if (current != assembly)
				{
					result = GetCopyrightInfo(current);
				}
			}

			return result;
		}

		/// <summary>
		/// Gets the shell's file type name and icon.
		/// </summary>
		/// <param name="fileName">A file name.  If <paramref name="useExistingFile"/> is true, then this should be
		/// the full path to an existing file.  Otherwise, this can just be an extension (with leading period) to get
		/// generic file type information.</param>
		/// <param name="useExistingFile">If true, then the file specified by <paramref name="fileName"/> must exist,
		/// and its icon will be extracted, which can be unique for some file types like .exe and .ico.  If false, then the
		/// <paramref name="fileName"/> can just be an extension, and the file type's standard icon will be used.</param>
		/// <param name="iconOptions">Options that determine the size and style of icon to get.</param>
		/// <param name="useIconHandle">If <paramref name="iconOptions"/> is None, then this should be null.
		/// Otherwise, this should be a delegate that converts the passed-in HICON into the desired icon type.
		/// In Windows Forms, this can be done with a delegate like:
		/// <c>hIcon => icon = (Icon)Icon.FromHandle(hIcon).Clone()</c>
		/// In WPF, the icon handle can be converted using the Imaging.CreateBitmapSourceFromHIcon method.
		/// <para/>
		/// Note: The icon handle will be destroyed automatically when this method returns, so the delegate
		/// must copy/clone the icon if it needs to use it later.
		/// </param>
		/// <returns>The shell's file type name (e.g., "Visual C# Source file" for a ".cs" file).</returns>
		public static string? GetFileTypeInfo(string fileName, bool useExistingFile, IconOptions iconOptions, Action<IntPtr>? useIconHandle)
			=> NativeMethods.GetShellFileTypeAndIcon(fileName, useExistingFile, iconOptions, useIconHandle);

		/// <summary>
		/// Gets the assembly's version information in a displayable format (e.g., for an About box).
		/// </summary>
		/// <param name="assembly">The assembly to get the version information from.</param>
		/// <returns>User-friendly version information.</returns>
		public static string GetVersionInfo(Assembly assembly)
		{
			Conditions.RequireReference(assembly, nameof(assembly));

			StringBuilder sb = new("Version ");

			// Show at least Major.Minor, but only show Build and Revision if they're non-zero.
			Version? displayVersion = ReflectionUtility.GetVersion(assembly);
			const int MaxVersionFields = 4;
			int versionFieldsToDisplay = MaxVersionFields;
			if (displayVersion != null)
			{
				if (displayVersion.Revision == 0)
				{
					versionFieldsToDisplay--;
				}

				sb.Append(displayVersion.ToString(versionFieldsToDisplay));
			}

			DateTime? built = ReflectionUtility.GetBuildTime(assembly);
			if (built != null)
			{
				DateTime buildTime = built.Value;

				// Debug builds usually only put a UTC date in the "BuildTime", so we don't need to localize it.
				if (buildTime.TimeOfDay != TimeSpan.Zero || !ReflectionUtility.IsDebugBuild(assembly))
				{
					buildTime = buildTime.ToLocalTime();
				}

				sb.Append(" – ").AppendFormat("{0:d}", buildTime);
			}

			sb.Append(Environment.Is64BitProcess ? " – 64-bit" : " – 32-bit");

			TargetFrameworkAttribute? frameworkAttribute = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
			if (frameworkAttribute != null && frameworkAttribute.FrameworkName.IsNotWhiteSpace())
			{
				// Examples:
				// [assembly: TargetFramework(".NETCoreApp,Version=v3.1", FrameworkDisplayName = "")]
				// [assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = "")]
				// [assembly: TargetFramework(".NETFramework,Version=v4.5", FrameworkDisplayName = ".NET Framework 4.5")]
				// [assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
				// [assembly: TargetFramework(".NETStandard,Version=v2.0", FrameworkDisplayName = "")]
				string frameworkName = frameworkAttribute.FrameworkName;
				const string Prefix = ".NET";
				int commaIndex = frameworkName.IndexOf(',');
				if (frameworkName.StartsWith(Prefix) && commaIndex > Prefix.Length)
				{
					string target = frameworkName.Substring(Prefix.Length, commaIndex - Prefix.Length);
					const string Suffix = "App";
					if (target.EndsWith(Suffix))
					{
						target = target.Substring(0, target.Length - Suffix.Length);
					}

					sb.Append(" – ").Append(target);
				}
			}

			// On Vista or later, show whether the user is running as an administrator.
			OperatingSystem os = Environment.OSVersion;
			const int WindowsVistaMajorVersion = 6;
			if (ApplicationInfo.IsWindows && os.Version >= new Version(WindowsVistaMajorVersion, 0) && ApplicationInfo.IsUserRunningAsAdministrator)
			{
				sb.Append(" – Administrator");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Searches the directories in the system PATH environment variable for the specified file and returns the
		/// fully qualified path if found.
		/// </summary>
		/// <remarks>The search preserves the order of directories as specified in the PATH environment variable and
		/// ignores duplicate entries, comparing paths in an OS-appropriate manner. Only the first matching file is returned.</remarks>
		/// <param name="fileName">The name of the file to locate within the directories specified by the PATH environment variable.
		/// Cannot be null or empty.</param>
		/// <returns>The fully qualified path to <paramref name="fileName"/> if it is found in one of the PATH directories;
		/// otherwise, null.</returns>
		public static string? SearchPath(string fileName)
		{
			string? result = null;

			string[] pathEntries = [.. (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
				.Split([Path.PathSeparator], StringSplitOptions.RemoveEmptyEntries)
				.Select(entry => entry.Trim())
				.Where(entry => !string.IsNullOrWhiteSpace(entry))];

			// The check for duplicates saves time here. On my system, 12 of the 55 entries in PATH are duplicates (ignoring case).
			// Unfortunately, we can't use LINQ's Distinct() because it returns an unordered sequence, and we have to preserve order.
			// Note: Windows 10 supports case-sensitive folders, but the PowerShell folders shouldn't be configured that way.
			HashSet<string> checkedPaths = new(ApplicationInfo.IsWindows ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
			foreach (string path in pathEntries)
			{
				if (checkedPaths.Add(path))
				{
					string fullyQualifiedName = Path.Combine(path, fileName);
					if (File.Exists(fullyQualifiedName))
					{
						result = fullyQualifiedName;
						break;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Executes the default action on the specified file using the Windows shell.
		/// </summary>
		/// <param name="ownerHandle">The parent window handle to use for any error dialogs.</param>
		/// <param name="fileName">The text or filename to execute.</param>
		/// <returns>The process started by executing the file.</returns>
		/// <exception cref="Win32Exception">An error occurred when opening the associated file.</exception>
		public static Process? ShellExecute(IntPtr? ownerHandle, string fileName) => ShellExecute(ownerHandle, fileName, string.Empty);

		/// <summary>
		/// Executes an action on the specified file using the Windows shell.
		/// </summary>
		/// <param name="ownerHandle">The parent window handle to use for any error dialogs.</param>
		/// <param name="fileName">The text or filename to execute.</param>
		/// <param name="verb">The shell action that should be taken.  Pass an empty string for the default action.</param>
		/// <returns>The process started by executing the file.</returns>
		/// <exception cref="Win32Exception">An error occurred when opening the associated file.</exception>
		public static Process? ShellExecute(IntPtr? ownerHandle, string fileName, string verb)
		{
			Conditions.RequireString(fileName, nameof(fileName));

			ProcessStartInfo startInfo = new() { ErrorDialog = true };
			if (ownerHandle != null)
			{
				startInfo.ErrorDialogParentHandle = ownerHandle.Value;
			}

			startInfo.FileName = fileName;
			startInfo.UseShellExecute = true;
			startInfo.Verb = verb;

			Process? result = Process.Start(startInfo);
			return result;
		}

		#endregion
	}
}
