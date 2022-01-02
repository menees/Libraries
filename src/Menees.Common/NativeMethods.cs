namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using System.Text;
	using Menees.Shell;

	#endregion

	internal static class NativeMethods
	{
		#region Private Enums

		[Flags]
		private enum ErrorModes : uint // Base as uint since SetErrorMode takes a UINT.
		{
			SYSTEM_DEFAULT = 0x0,
			SEM_FAILCRITICALERRORS = 0x0001,
			SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
			SEM_NOGPFAULTERRORBOX = 0x0002,
			SEM_NOOPENFILEERRORBOX = 0x8000,
		}

		[Flags]
		private enum SHGFI : int
		{
			Icon = 0x000000100,
			DisplayName = 0x000000200,
			TypeName = 0x000000400,
			Attributes = 0x000000800,
			IconLocation = 0x000001000,
			ExeType = 0x000002000,
			SysIconIndex = 0x000004000,
			LinkOverlay = 0x000008000,
			Selected = 0x000010000,
			Attr_Specified = 0x000020000,
			LargeIcon = 0x000000000,
			SmallIcon = 0x000000001,
			OpenIcon = 0x000000002,
			ShellIconSize = 0x000000004,
			PIDL = 0x000000008,
			UseFileAttributes = 0x000000010,
			AddOverlays = 0x000000020,
			OverlayIndex = 0x000000040,
		}

		#endregion

		#region Internal Methods

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeleteFile(string fileName);

		internal static void DisableShellModalErrorDialogs()
		{
			if (ApplicationInfo.IsWindows)
			{
				const ErrorModes newErrorMode = ErrorModes.SEM_FAILCRITICALERRORS |
					ErrorModes.SEM_NOGPFAULTERRORBOX |
					ErrorModes.SEM_NOOPENFILEERRORBOX;
				SetErrorMode(newErrorMode);
			}
		}

		internal static string GetModuleFileName(IntPtr module)
		{
			const int BufferSize = 32768;
			StringBuilder sb = new(BufferSize);
			uint result = GetModuleFileName(module, sb, sb.Capacity);
			if (result == 0)
			{
				throw Exceptions.Log(new Win32Exception(Marshal.GetLastWin32Error(), "Error calling GetModuleFileName for the specified HMODULE."));
			}
			else
			{
				string fileName = sb.ToString();

				// See the docs for GetModuleFileName and the "Naming a File" MSDN topic.
				const string longUncPrefix = @"\\?\UNC\";
				const string longPrefix = @"\\?\";
				if (fileName.StartsWith(longUncPrefix))
				{
					fileName = fileName.Substring(longUncPrefix.Length);
				}
				else if (fileName.StartsWith(longPrefix))
				{
					fileName = fileName.Substring(longPrefix.Length);
				}

				return fileName;
			}
		}

		internal static string[] SplitCommandLine(string commandLine)
		{
			// This logic came from http://www.pinvoke.net/default.aspx/shell32/CommandLineToArgvW.html
			// and also from http://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp.
			// Use CommandLineToArgvW because it handles a variety of special quote and backslash cases such as:
			// 		/c:"1 2" is parsed as a single arg: /c:1 2
			// 		Double double quotes ("") become a single escaped double quote usually.
			// 		Backslash double quote may be parsed as an escaped double quote.
			// See: "What's up with the strange treatment of quotation marks and backslashes by CommandLineToArgvW"
			// http://blogs.msdn.com/b/oldnewthing/archive/2010/09/17/10063629.aspx
			// Also: "How is the CommandLineToArgvW function intended to be used?"
			// http://blogs.msdn.com/b/oldnewthing/archive/2010/09/16/10062818.aspx
			// And: "The first word on the command line is the program name only by convention"
			// http://blogs.msdn.com/b/oldnewthing/archive/2006/05/15/597984.aspx
			IntPtr ptrToSplitArgs = CommandLineToArgvW(commandLine, out int numberOfArgs);

			// CommandLineToArgvW returns NULL upon failure.
			if (ptrToSplitArgs == IntPtr.Zero)
			{
				// The inner Win32Exception will use the last error code set by CommandLineToArgvW.
				throw Exceptions.Log(new ArgumentException("Unable to split command line: " + commandLine, new Win32Exception()));
			}

			// Make sure the memory ptrToSplitArgs to is freed, even upon failure.
			try
			{
				string[] splitArgs = new string[numberOfArgs];

				// ptrToSplitArgs is an array of pointers to null terminated Unicode strings.
				// Copy each of these strings into our split argument array.
				for (int i = 0; i < numberOfArgs; i++)
				{
					IntPtr lpwstr = Marshal.ReadIntPtr(ptrToSplitArgs, i * IntPtr.Size);
					splitArgs[i] = Marshal.PtrToStringUni(lpwstr)!;
				}

				return splitArgs;
			}
			finally
			{
				// Free memory obtained by CommandLineToArgW.
				// In .NET the LocalFree API is exposed by Marshal.FreeHGlobal.
				Marshal.FreeHGlobal(ptrToSplitArgs);
			}
		}

		internal static string? GetShellFileTypeAndIcon(string fileName, bool useExistingFile, IconOptions iconOptions, Action<IntPtr>? useIconHandle)
		{
			Conditions.RequireString(fileName, nameof(fileName));
			Conditions.RequireArgument(
				(iconOptions == IconOptions.None && useIconHandle == null) || (iconOptions != IconOptions.None && useIconHandle != null),
				"The iconOptions and useIconHandle arguments must be compatible.");

			SHFILEINFO info = default;
			int cbFileInfo = Marshal.SizeOf(info);
			SHGFI flags = SHGFI.TypeName;

			int fileAttributes = 0;
			if (!useExistingFile)
			{
				flags |= SHGFI.UseFileAttributes;
				const int FILE_ATTRIBUTE_NORMAL = 128;
				const int FILE_ATTRIBUTE_DIRECTORY = 16;

				// http://stackoverflow.com/questions/1599235/how-do-i-fetch-the-folder-icon-on-windows-7-using-shell32-shgetfileinfo
				fileAttributes = iconOptions.HasFlag(IconOptions.Folder) ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL;
			}

			bool useIcon = iconOptions != IconOptions.None;
			if (useIcon)
			{
				flags |= SHGFI.Icon;

				// The Small and Large options are mutually exclusive.  If they specify neither or both, then we'll default to small.
				if (iconOptions.HasFlag(IconOptions.Small) || !iconOptions.HasFlag(IconOptions.Large))
				{
					flags |= SHGFI.SmallIcon;
				}
				else
				{
					flags |= SHGFI.LargeIcon;
				}

				if (iconOptions.HasFlag(IconOptions.ShellSize))
				{
					flags |= SHGFI.ShellIconSize;
				}

				if (iconOptions.HasFlag(IconOptions.Open))
				{
					flags |= SHGFI.OpenIcon;
				}

				if (iconOptions.HasFlag(IconOptions.Shortcut))
				{
					flags |= SHGFI.LinkOverlay;
				}

				if (iconOptions.HasFlag(IconOptions.Selected))
				{
					flags |= SHGFI.Selected;
				}
			}

			string? result = null;
			if (SHGetFileInfo(fileName, fileAttributes, out info, (uint)cbFileInfo, flags) != IntPtr.Zero)
			{
				result = info.szTypeName;
				if (useIcon && useIconHandle != null)
				{
					// The caller has to make a copy (e.g., Icon.FromHandle(hIcon).Clone()).
					useIconHandle(info.hIcon);
					DestroyIcon(info.hIcon);
				}
			}

			return result;
		}

		[DllImport("libc", EntryPoint = "getuid")]
		internal static extern uint GetUid();

		#endregion

		#region Private Extern Methods

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern uint GetModuleFileName([In] IntPtr module, [Out] StringBuilder fileNameBuffer, [In, MarshalAs(UnmanagedType.U4)] int bufferSize);

		[DllImport("kernel32.dll")]
		private static extern ErrorModes SetErrorMode(ErrorModes uMode);

		[DllImport("shell32.dll", SetLastError = true)]
		private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes, out SHFILEINFO psfi, uint cbfileInfo, SHGFI uFlags);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DestroyIcon(IntPtr handle);

		#endregion

		#region Private Types

		#region SHFILEINFO

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
			public string szDisplayName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		#endregion

		#endregion
	}
}