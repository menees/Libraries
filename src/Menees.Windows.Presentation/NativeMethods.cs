namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Windows;
	using System.Windows.Interop;
	using IntPoint = System.Drawing.Point;

	#endregion

	internal static class NativeMethods
	{
		#region Private Data Members

		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;
		private const int SW_SHOWMAXIMIZED = 3;
		private const int SWP_NOSIZE = 0x0001;
		private const int SWP_NOMOVE = 0x0002;

		private static readonly IntPtr HWND_BOTTOM = new(1);

		#endregion

		#region Public Methods

		public static void RemoveIcon(Window window)
		{
			// From http://www.wpftutorial.net/RemoveIcon.html
			// and http://stackoverflow.com/questions/2341230/removing-icon-from-a-wpf-window
			// and http://stackoverflow.com/questions/3096359/wpf-remove-system-menu-icon-from-modal-window-but-not-main-app-window?rq=1
			IntPtr hwnd = GetHandle(window);

			// Change the extended window style to not show a window icon
			const int GWL_EXSTYLE = -20;
			const int WS_EX_DLGMODALFRAME = 0x0001;
			int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
			if (SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME) != 0)
			{
				// Reset the icon, both calls important
				const uint WM_SETICON = 0x0080;
				const int ICON_SMALL = 0;
				const int ICON_BIG = 1;
				SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_SMALL, IntPtr.Zero);
				SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_BIG, IntPtr.Zero);

				// Update the window's non-client area to reflect the changes
				const int SWP_NOZORDER = 0x0004;
				const int SWP_FRAMECHANGED = 0x0020;
				SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
			}
		}

		public static void SendToBack(Window window)
		{
			// From http://stackoverflow.com/questions/1181336/how-to-send-a-wpf-window-to-the-back
			// and http://stackoverflow.com/questions/18627445/send-to-back-in-wpf-window
			IntPtr hwnd = GetHandle(window);
			const uint SWP_NOACTIVATE = 0x0010;
			SetWindowPos(hwnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
		}

		public static void LoadWindowPlacement(Window window, ISettingsNode node, WindowState? stateOverride)
		{
			// Most of this logic came from:
			// http://blogs.msdn.com/b/davidrickard/archive/2010/03/09/saving-window-size-and-location-in-wpf-and-winforms.aspx
			// Other ideas came from the old FormSaver code and from:
			// http://stackoverflow.com/questions/847752/net-wpf-remember-window-size-between-sessions
			WINDOWPLACEMENT placement = WINDOWPLACEMENT.Create();
			placement.Load(node);

			switch (stateOverride)
			{
				case WindowState.Normal:
					placement.showCmd = SW_SHOWNORMAL;
					break;

				case WindowState.Maximized:
					placement.showCmd = SW_SHOWMAXIMIZED;
					break;

				case WindowState.Minimized:
					placement.showCmd = SW_SHOWMINIMIZED;
					break;

				default:
					// If the window was previously minimized, then show it as normal.
					if (placement.showCmd == SW_SHOWMINIMIZED)
					{
						placement.showCmd = SW_SHOWNORMAL;
					}

					break;
			}

			if (!SetWindowPlacement(GetHandle(window), ref placement))
			{
				throw CreateExceptionForLastError();
			}
		}

		public static void SaveWindowPlacement(Window window, ISettingsNode node)
		{
			WINDOWPLACEMENT placement = WINDOWPLACEMENT.Create();

			if (!GetWindowPlacement(GetHandle(window), ref placement))
			{
				throw CreateExceptionForLastError();
			}

			placement.Save(node);
		}

		public static IntPtr GetHandle(Window window)
		{
			var helper = new WindowInteropHelper(window);
			helper.EnsureHandle();
			IntPtr result = helper.Handle;
			return result;
		}

		#endregion

		#region Private Methods

		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hwnd, int index);

		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

		private static Win32Exception CreateExceptionForLastError()
		{
			Win32Exception result = Exceptions.Log(new Win32Exception(Marshal.GetLastWin32Error()));
			return result;
		}

		#endregion

		#region Private Types

		// RECT structure required by WINDOWPLACEMENT structure
		// Note: This is NOT the same as System.Drawing.Rectangle.
		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		// WINDOWPLACEMENT stores the position, size, and state of a window
		[StructLayout(LayoutKind.Sequential)]
		private struct WINDOWPLACEMENT
		{
#pragma warning disable CC0074 // Make field readonly
			public int length;
			public int flags;
			public int showCmd;
			public IntPoint minPosition;
			public IntPoint maxPosition;
			public RECT normalPosition;
#pragma warning restore CC0074 // Make field readonly

			#region Public Methods

			public static WINDOWPLACEMENT Create()
			{
				WINDOWPLACEMENT result = default;
				result.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
				return result;
			}

			public void Load(ISettingsNode node)
			{
				// The length field should always be calculated by SizeOf, and flags should remain at 0.
				this.showCmd = node.GetValue("ShowCmd", SW_SHOWNORMAL);
				this.minPosition.X = node.GetValue("MinPosition.X", 0);
				this.minPosition.Y = node.GetValue("MinPosition.Y", 0);
				this.maxPosition.X = node.GetValue("MaxPosition.X", 0);
				this.maxPosition.Y = node.GetValue("MaxPosition.Y", 0);
				this.normalPosition.Left = node.GetValue("NormalPosition.Left", 0);
				this.normalPosition.Top = node.GetValue("NormalPosition.Top", 0);
				this.normalPosition.Right = node.GetValue("NormalPosition.Right", 0);
				this.normalPosition.Bottom = node.GetValue("NormalPosition.Bottom", 0);
			}

			public void Save(ISettingsNode node)
			{
				// The length and flags fields should not be saved.
				node.SetValue("ShowCmd", this.showCmd);
				node.SetValue("MinPosition.X", this.minPosition.X);
				node.SetValue("MinPosition.Y", this.minPosition.Y);
				node.SetValue("MaxPosition.X", this.maxPosition.X);
				node.SetValue("MaxPosition.Y", this.maxPosition.Y);
				node.SetValue("NormalPosition.Left", this.normalPosition.Left);
				node.SetValue("NormalPosition.Top", this.normalPosition.Top);
				node.SetValue("NormalPosition.Right", this.normalPosition.Right);
				node.SetValue("NormalPosition.Bottom", this.normalPosition.Bottom);
			}

			#endregion
		}

		#endregion
	}
}
