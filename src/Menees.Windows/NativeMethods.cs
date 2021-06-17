namespace Menees.Windows
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

	internal static partial class NativeMethods
	{
		#region Private Data Members

		private const string CLSID_FileOpenDialog = "DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7";
		private const string IID_IFileDialog = "42F85136-DB7E-439C-85F1-E4075D135FC8";
		private const string IID_IShellItem = "43826D1E-E718-42EE-BC55-A1E261C37BFE";
		private const uint S_OK = 0;

		#endregion

		#region Private Interfaces

		#region IFileDialog

		[ComImport]
		[Guid(IID_IFileDialog)]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IFileDialog
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[PreserveSig] // Need this in case it returns ERROR_CANCELLED, which shouldn't be an exception.
			uint Show([In, Optional] IntPtr hwndOwner); // From IModalWindow

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint SetFileTypes([In] uint cFileTypes, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr rgFilterSpec);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint SetFileTypeIndex([In] uint iFileType);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint GetFileTypeIndex(out uint piFileType);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint Advise([In, MarshalAs(UnmanagedType.Interface)] IntPtr pfde, out uint pdwCookie);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint Unadvise([In] uint dwCookie);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint SetOptions([In] uint fos);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint GetOptions(out uint fos);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint SetFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, uint fdap);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint Close([MarshalAs(UnmanagedType.Error)] uint hr);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint SetClientGuid([In] ref Guid guid);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint ClearClientData();

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
		}

		#endregion

		#region IShellItem

		[ComImport]
		[Guid(IID_IShellItem)]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IShellItem
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint BindToHandler([In] IntPtr pbc, [In] ref Guid rbhid, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out IntPtr ppvOut);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint GetParent([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint GetDisplayName([In] uint sigdnName, out IntPtr ppszName);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint GetAttributes([In] uint sfgaoMask, out uint psfgaoAttribs);

			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			uint Compare([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, [In] uint hint, out int piOrder);
		}

		#endregion

		#endregion

		#region Internal Properties

		[SuppressMessage(
			"Microsoft.Usage",
			"CA1806:DoNotIgnoreMethodResults",
			MessageId = "Menees.NativeMethods.GetWindowThreadProcessId(System.IntPtr,System.Int32@)",
			Justification = "GetWindowThreadProcessId returns a thread ID not an HRESULT.  We check Marshal.GetLastWin32Error().")]
		internal static bool IsApplicationActivated
		{
			get
			{
				// This property is thread-safe, not WinForm or WPF specific, and works even if child windows have focus in a process.
				// http://stackoverflow.com/questions/7162834/determine-if-current-application-is-activated-has-focus/7162873#7162873
				bool result = false;

				// GetForegroundWindow will return null if no window is currently activated
				// (e.g., the screen is locked or a remote desktop is connected but minimized).
				IntPtr activatedHandle = GetForegroundWindow();
				if (Marshal.GetLastWin32Error() == 0 && activatedHandle != IntPtr.Zero)
				{
					GetWindowThreadProcessId(activatedHandle, out int activatedProcessId);
					result = Marshal.GetLastWin32Error() == 0 && activatedProcessId == ApplicationInfo.ProcessId;
				}

				return result;
			}
		}

		#endregion

		#region Internal Methods

		[SuppressMessage(
			"Microsoft.Usage",
			"CA2201:DoNotRaiseReservedExceptionTypes",
			Justification = "We have to use PreserveSig, check for S_OK or ERROR_CANCELLED, and throw otherwise.")]
		internal static string SelectFolder(IntPtr? ownerHandle, string title, string initialFolder)
		{
			// This uses the "new" IFileDialog implementation rather than the old, awful SHBrowseForFolder dialog.
			// This implementation was pieced together from several C# and C++ examples:
			// http://www.jmedved.com/2011/12/openfolderdialog/
			// http://stackoverflow.com/a/15386992/1882616
			// http://stackoverflow.com/questions/600346/using-openfiledialog-for-directory-not-folderbrowserdialog
			// http://msdn.microsoft.com/en-us/library/windows/desktop/bb776913(v=vs.85).aspx
			//
			// Note: Because we're using strongly-typed interfaces here for IFileDialog and IShellItem,
			// we don't have to mess with Marshal.ReleaseComObject in this method.  .NET will manage
			// the lifetimes of the RCWs that it creates.
			IFileDialog dialog = (IFileDialog)new FileOpenDialog();

			// The MSDN examples say we should always do GetOptions before SetOptions to avoid overriding default options.
			dialog.GetOptions(out uint options);

			const uint FOS_PICKFOLDERS = 0x00000020;
			const uint FOS_FORCEFILESYSTEM = 0x00000040;
			const uint FOS_NOVALIDATE = 0x00000100;
			const uint FOS_NOTESTFILECREATE = 0x00010000;
			const uint FOS_DONTADDTORECENT = 0x02000000;
			options |= FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM | FOS_NOVALIDATE | FOS_NOTESTFILECREATE | FOS_DONTADDTORECENT;
			dialog.SetOptions(options);

			// Default to the current directory in case the initial folder is missing or invalid.
			// Windows only uses this for the first usage of the dialog with the current title
			// in the current process's path.  For subsequent calls, Windows remembers
			// and starts from the previous path.
			IShellItem currentDirectoryItem = GetShellItemForPath(Environment.CurrentDirectory);
			if (currentDirectoryItem != null)
			{
				dialog.SetDefaultFolder(currentDirectoryItem);
			}

			if (!string.IsNullOrEmpty(initialFolder))
			{
				IShellItem initialFolderItem = GetShellItemForPath(initialFolder);
				if (initialFolderItem != null)
				{
					dialog.SetFolder(initialFolderItem);
				}
			}

			if (!string.IsNullOrEmpty(title))
			{
				dialog.SetTitle(title);
			}

			uint showResult = dialog.Show(ownerHandle.GetValueOrDefault());
			const uint ERROR_CANCELLED = 0x800704C7; // For when the user clicks Cancel.
			string result = null;
			if (showResult == S_OK)
			{
				if (dialog.GetResult(out IShellItem shellItem) == S_OK)
				{
					const uint SIGDN_FILESYSPATH = 0x80058000;
					if (shellItem.GetDisplayName(SIGDN_FILESYSPATH, out IntPtr pszString) == S_OK)
					{
						if (pszString != IntPtr.Zero)
						{
							try
							{
								result = Marshal.PtrToStringAuto(pszString);
							}
							finally
							{
								Marshal.FreeCoTaskMem(pszString);
							}
						}
					}
				}
			}
			else if (showResult != ERROR_CANCELLED)
			{
				throw new COMException("An error occurred while showing the Select Folder dialog.", unchecked((int)showResult));
			}

			return result;
		}

		internal static void BringWindowForward(IntPtr hWnd)
		{
			if (IsIconic(hWnd))
			{
				const int SW_RESTORE = 9;
				ShowWindowAsync(hWnd, SW_RESTORE);
			}

			SetForegroundWindow(hWnd);
		}

		internal static bool PostShowWindowCommand(IntPtr hWnd, ShowWindowCommand command)
			=> ShowWindowAsync(hWnd, (int)command);

		#endregion

		#region Private Methods

		private static IShellItem GetShellItemForPath(string path)
		{
			var riid = new Guid(IID_IShellItem);
			if (SHCreateItemFromParsingName(path, IntPtr.Zero, ref riid, out IShellItem result) != S_OK)
			{
				// If the user types an invalid "path" into an input box, we don't want to raise an exception.
				result = null;
			}

			return result;
		}

		#endregion

		#region Private Extern Methods

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern int SHCreateItemFromParsingName(
			[MarshalAs(UnmanagedType.LPWStr)] string pszPath,
			IntPtr pbc,
			ref Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		[SuppressMessage("", "CC0072", Justification = "The Async suffix comes from the Win32 API.")]
		private static extern bool ShowWindowAsync(IntPtr hWnd, int commandShow);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

		#endregion

		#region Private Types

		#region FileOpenDialog

		// Microsoft defines this coclass.  We can't declare it as sealed because then the compiler won't allow us to cast it to IFileDialog.
		[Guid(CLSID_FileOpenDialog)]
		[ComImport]
		[ClassInterface(ClassInterfaceType.None)]
		[TypeLibType(TypeLibTypeFlags.FCanCreate)]
		private class FileOpenDialog
		{
		}

		#endregion

		#endregion
	}
}