namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Interop;
	using System.Windows.Media;
	using System.Windows.Threading;
	using Menees.Shell;

	#endregion

	/// <summary>
	/// Methods and properties for Windows applications.
	/// </summary>
	public static class WindowsUtility
	{
		#region Public Properties

		/// <summary>
		/// Gets whether the clipboard currently contains text data.
		/// </summary>
		public static bool ClipboardContainsText
		{
			get
			{
				bool result = false;

				try
				{
					IDataObject data = Clipboard.GetDataObject();
					if (data != null)
					{
						result = data.GetDataPresent(DataFormats.Text);
					}
				}
				catch (ExternalException)
				{
					result = false;
				}

				return result;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Tries to bring the specified window to the front of the Z-order and activate it.
		/// </summary>
		/// <param name="window">The window to activate.</param>
		public static void BringToFront(Window window)
		{
			Conditions.RequireReference(window, () => window);

			// This came from http://stackoverflow.com/questions/257587/bring-a-window-to-the-front-in-wpf/4831839#4831839
			if (!window.IsVisible)
			{
				window.Show();
			}

			if (window.WindowState == WindowState.Minimized)
			{
				window.WindowState = WindowState.Normal;
			}

			window.Activate();
			if (!window.Topmost)
			{
				window.Topmost = true;
				window.Topmost = false;
			}

			window.Focus();
		}

		/// <summary>
		/// Gets a reference to the Window that hosts the content tree that contains the dependency object.
		/// </summary>
		/// <param name="dependencyObject">A dependency object or null.</param>
		/// <returns>A Window reference to the host window, or null if <paramref name="dependencyObject"/> is null.</returns>
		public static Window GetWindow(DependencyObject dependencyObject)
		{
			Window result = null;

			if (dependencyObject != null)
			{
				result = Window.GetWindow(dependencyObject);
			}

			return result;
		}

		/// <summary>
		/// Initializes the application's name and sets up a handler to report uncaught
		/// exceptions.
		/// </summary>
		/// <param name="applicationName">The name of the application to pass to <see cref="ApplicationInfo.Initialize"/>.</param>
		/// <param name="showException">The action to call when an exception needs to be shown.  This can be null,
		/// which will cause <see cref="ShowError(Window,string)"/> to be called.</param>
		/// <param name="applicationAssembly">The assembly that's initializing the application, typically the main executable.</param>
		public static void InitializeApplication(string applicationName, Action<Exception> showException, Assembly applicationAssembly = null)
		{
			ApplicationInfo.Initialize(applicationName, applicationAssembly ?? Assembly.GetCallingAssembly());

			Application.Current.DispatcherUnhandledException += (sender, e) =>
			{
				e.Handled = true;
				Exception ex = e.Exception;
				Log.Error(typeof(WindowsUtility), "An unhandled exception occurred in a Windows thread.", ex);
				ApplicationInfo.ShowUnhandledException(ex, message => e.Dispatcher.BeginInvoke(new Action(() => ShowError(null, message))), showException);
			};
		}

		/// <summary>
		/// Gets whether the specified window is in design mode.
		/// </summary>
		public static bool IsInDesignMode(DependencyObject dependencyObject)
		{
			Conditions.RequireReference(dependencyObject, () => dependencyObject);

			// http://stackoverflow.com/questions/834283/is-there-a-way-to-check-if-wpf-is-currently-executing-in-design-mode-or-not
			bool result = DesignerProperties.GetIsInDesignMode(dependencyObject);
			return result;
		}

		/// <summary>
		/// Selects a file system path and allows the user to type in a path if necessary.
		/// </summary>
		/// <param name="owner">The owner of the displayed modal dialog.</param>
		/// <param name="title">A short title for the path being selected.</param>
		/// <param name="initialFolder">The initial path to select.</param>
		/// <returns>The path the user selected if they pressed OK.  Null otherwise (e.g., the user cancelled).</returns>
		public static string SelectFolder(DependencyObject owner, string title, string initialFolder) => SelectFolder(GetWindow(owner), title, initialFolder);

		/// <summary>
		/// Selects a file system path and allows the user to type in a path if necessary.
		/// </summary>
		/// <param name="owner">The owner of the displayed modal dialog.</param>
		/// <param name="title">A short title for the path being selected.</param>
		/// <param name="initialFolder">The initial path to select.</param>
		/// <returns>The path the user selected if they pressed OK.  Null otherwise (e.g., the user cancelled).</returns>
		public static string SelectFolder(Window owner, string title, string initialFolder)
		{
			IntPtr? ownerHandle = null;
			if (owner != null)
			{
				ownerHandle = NativeMethods.GetHandle(owner);
			}

			string result = HandleUtility.SelectFolder(ownerHandle, title, initialFolder);
			return result;
		}

		/// <summary>
		/// Moves the window behind other top-level windows.
		/// </summary>
		/// <param name="window">The window to move to the bottom of the Z-order.</param>
		public static void SendToBack(Window window)
		{
			Conditions.RequireReference(window, () => window);
			NativeMethods.SendToBack(window);
		}

		/// <summary>
		/// Executes the default action on the specified file using the Windows shell.
		/// </summary>
		/// <param name="dependencyObject">A dependency object that can be used to find
		/// the parent window for any error dialogs.</param>
		/// <param name="fileName">The text or filename to execute.</param>
		/// <returns>Whether the file was opened/executed successfully.</returns>
		public static bool ShellExecute(DependencyObject dependencyObject, string fileName)
			=> ShellExecute(GetWindow(dependencyObject), fileName);

		/// <summary>
		/// Executes an action on the specified file using the Windows shell.
		/// </summary>
		/// <param name="dependencyObject">A dependency object that can be used to find
		/// the parent window for any error dialogs.</param>
		/// <param name="fileName">The text or filename to execute.</param>
		/// <param name="verb">The shell action that should be taken.  Pass an empty string for the default action.</param>
		/// <returns>The process started by executing the file.</returns>
		public static Process ShellExecute(DependencyObject dependencyObject, string fileName, string verb)
			=> ShellExecute(GetWindow(dependencyObject), fileName, verb);

		/// <summary>
		/// Executes the default action on the specified file using the Windows shell.
		/// </summary>
		/// <param name="owner">The parent window for any error dialogs.</param>
		/// <param name="fileName">The text or filename to execute.</param>
		/// <returns>Whether the file was opened/executed successfully.</returns>
		public static bool ShellExecute(Window owner, string fileName)
		{
			bool result = false;

			try
			{
				using (Process process = ShellExecute(owner, fileName, string.Empty))
				{
					result = true;
				}
			}
#pragma warning disable CC0004 // Catch block cannot be empty
			catch (Win32Exception)
			{
				// The core ShellExecute logic already displays an error dialog if a Win32Exception occurs.
			}
#pragma warning restore CC0004 // Catch block cannot be empty

			return result;
		}

		/// <summary>
		/// Executes an action on the specified file using the Windows shell.
		/// </summary>
		/// <param name="owner">The parent window for any error dialogs.</param>
		/// <param name="fileName">The text or filename to execute.</param>
		/// <param name="verb">The shell action that should be taken.  Pass an empty string for the default action.</param>
		/// <returns>The process started by executing the file.</returns>
		public static Process ShellExecute(Window owner, string fileName, string verb)
		{
			IntPtr? ownerHandle = null;
			if (owner != null)
			{
				ownerHandle = NativeMethods.GetHandle(owner);
			}

			Process result = ShellUtility.ShellExecute(ownerHandle, fileName, verb);
			return result;
		}

		/// <summary>
		/// Displays a standard "About" dialog for the current application assembly.
		/// </summary>
		/// <remarks>
		/// At application startup, you should call the <see cref="InitializeApplication"/>
		/// method to set the current application's name, which will be displayed in the
		/// About box.
		/// </remarks>
		/// <param name="owner">The owner of the displayed modal dialog.</param>
		/// <param name="mainAssembly">The main assembly for the application,
		/// which the version and copyright information will be read from.</param>
		public static void ShowAboutBox(Window owner, Assembly mainAssembly)
		{
			// If an assembly wasn't provided, then we want the version of the calling assembly not the current assembly.
			AboutBox dialog = new AboutBox(mainAssembly ?? Assembly.GetCallingAssembly());
			dialog.Execute(owner);
		}

		/// <summary>
		/// Displays an error message in a MessageBox.
		/// </summary>
		/// <param name="dependencyObject">A dependency object that can be used to find the owner window.
		/// This can be null to use the desktop as the owner.</param>
		/// <param name="message">The message to display.</param>
		/// <param name="caption">The caption to use as the dialog's title. If null, the application name is used.</param>
		public static void ShowError(DependencyObject dependencyObject, string message, string caption = null)
		{
			ShowError(GetWindow(dependencyObject), message, caption);
		}

		/// <summary>
		/// Displays an error message in a MessageBox.
		/// </summary>
		/// <param name="owner">The dialog owner window.  This can be null to use the desktop as the owner.</param>
		/// <param name="message">The message to display.</param>
		/// <param name="caption">The caption to use as the dialog's title. If null, the application name is used.</param>
		public static void ShowError(Window owner, string message, string caption = null)
		{
			if (caption == null)
			{
				caption = ApplicationInfo.ApplicationName;
			}

			// WPF's stupid MessageBox implementation throws an ArgumentNullException if we pass it a null owner.
			if (owner != null)
			{
				MessageBox.Show(owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>
		/// Displays an information message in a MessageBox.
		/// </summary>
		/// <param name="dependencyObject">A dependency object that can be used to find the owner window.
		/// This can be null to use the desktop as the owner.</param>
		/// <param name="message">The message to display.</param>
		/// <param name="caption">The caption to use as the dialog's title. If null, the application name is used.</param>
		public static void ShowInfo(DependencyObject dependencyObject, string message, string caption = null)
		{
			ShowInfo(GetWindow(dependencyObject), message, caption);
		}

		/// <summary>
		/// Displays an information message in a MessageBox.
		/// </summary>
		/// <param name="owner">The dialog owner window.  This can be null to use the desktop as the owner.</param>
		/// <param name="message">The message to display.</param>
		/// <param name="caption">The caption to use as the dialog's title. If null, the application name is used.</param>
		public static void ShowInfo(Window owner, string message, string caption = null)
		{
			if (caption == null)
			{
				caption = ApplicationInfo.ApplicationName;
			}

			// WPF's stupid MessageBox implementation throws an ArgumentNullException if we pass it a null owner.
			if (owner != null)
			{
				MessageBox.Show(owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
			}
			else
			{
				MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		/// <summary>
		/// Displays a yes/no question in a MessageBox.
		/// </summary>
		/// <param name="owner">The dialog owner window.  This can be null to use the desktop as the owner.</param>
		/// <param name="yesNoQuestion">The yes/no question to display.</param>
		/// <param name="caption">The caption to use as the dialog's title. If null, the application name is used.</param>
		/// <param name="defaultYes">Whether the default button should be Yes (instead of No).</param>
		public static bool ShowQuestion(Window owner, string yesNoQuestion, string caption = null, bool defaultYes = true)
		{
			if (caption == null)
			{
				caption = ApplicationInfo.ApplicationName;
			}

			MessageBoxResult defaultResult = defaultYes ? MessageBoxResult.Yes : MessageBoxResult.No;

			// WPF's stupid MessageBox implementation throws an ArgumentNullException if we pass it a null owner.
			MessageBoxResult mbResult;
			if (owner != null)
			{
				mbResult = MessageBox.Show(owner, yesNoQuestion, caption, MessageBoxButton.YesNo, MessageBoxImage.Question, defaultResult);
			}
			else
			{
				mbResult = MessageBox.Show(yesNoQuestion, caption, MessageBoxButton.YesNo, MessageBoxImage.Question, defaultResult);
			}

			bool result = mbResult == MessageBoxResult.Yes;
			return result;
		}

		/// <summary>
		/// Shows an input box for a single value with validation.
		/// </summary>
		/// <param name="owner">The owner of the displayed modal dialog.</param>
		/// <param name="prompt">The message to prompt with (up to 4 lines long).</param>
		/// <param name="title">The caption of the displayed dialog.  This can be null
		/// to use the application name as the title.</param>
		/// <param name="defaultValue">The initial value of the input field.</param>
		/// <param name="maxLength">The maximum length of the input field in characters.
		/// This can be null to use the default maximum length limit.</param>
		/// <param name="validate">An optional function to validate the input.  This can be null.
		/// The function should return a null if the input passes validation, and it should return an error
		/// message to display to the end user if the input fails validation.</param>
		/// <returns>The user-entered value if they pressed OK, or null if Cancel was pressed.</returns>
		public static string ShowInputBox(
			Window owner,
			string prompt,
			string title,
			string defaultValue,
			int? maxLength = null,
			Func<string, string> validate = null)
		{
			InputDialog dialog = new InputDialog();
			if (string.IsNullOrEmpty(title))
			{
				dialog.Title = ApplicationInfo.ApplicationName;
			}
			else
			{
				dialog.Title = title;
			}

			string result = dialog.Execute(owner, prompt, defaultValue, maxLength, validate);
			return result;
		}

		/// <summary>
		/// Gets the distinct set of <see cref="ValidationError"/> instances from the specified object's visual tree.
		/// </summary>
		/// <param name="dependencyObject">An object with a visual tree that can contain child items with
		/// validation errors (e.g., a DataGrid).</param>
		/// <returns>The unique ValidationError instances</returns>
		/// <remarks>
		/// This adds errors to an ISet because the same error can show up multiple times in the same visual tree.
		/// For example, when a DataGridRow has an error in a TextBox, both will report the same ValidationError.
		/// </remarks>
		public static ISet<ValidationError> GetValidationErrors(DependencyObject dependencyObject)
		{
			Conditions.RequireReference(dependencyObject, nameof(dependencyObject));
			HashSet<ValidationError> result = new HashSet<ValidationError>();
			GetValidationErrors(dependencyObject, result);
			return result;
		}

		#endregion

		#region Private Methods

		private static void GetValidationErrors(DependencyObject parent, ISet<ValidationError> errors)
		{
			foreach (ValidationError error in Validation.GetErrors(parent))
			{
				errors.Add(error);
			}

			int visualChildCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < visualChildCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);
				GetValidationErrors(child, errors);
			}
		}

		#endregion
	}
}
