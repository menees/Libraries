namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Interop;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using Menees.Diagnostics;
	using Menees.Shell;
	using IconImage = System.Drawing.Icon;

	#endregion

	/// <summary>
	/// Interaction logic for AboutBox.xaml
	/// </summary>
	internal partial class AboutBox : ExtendedDialog
	{
		#region Private Data Members

		private readonly Assembly callingAssembly;
		private readonly string repository;

		#endregion

		#region Constructors

		public AboutBox(Assembly callingAssembly, string? repository)
		{
			this.InitializeComponent();

			this.callingAssembly = callingAssembly;
			this.repository = repository ?? ApplicationInfo.ApplicationName.Replace(" ", string.Empty);

			string applicationName = ApplicationInfo.ApplicationName;
			this.Title = "About " + applicationName;
			this.productName.Text = applicationName;
			this.version.Text = ShellUtility.GetVersionInfo(callingAssembly);
			this.copyright.Text = ShellUtility.GetCopyrightInfo(callingAssembly);
			this.updateLink.Visibility = Visibility.Hidden;
		}

		#endregion

		#region Public Methods

		public void Execute(Window? owner)
		{
			// If there's no visible owner window, then this dialog should be centered on the screen.
			if (owner == null || owner.Visibility != Visibility.Visible)
			{
				this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				this.ShowInTaskbar = true;
			}

			// We can't do "this.icon.Source = owner.Icon;" because that always gets a small (e.g., 16x16) icon.
			using (IconImage? appIcon = IconImage.ExtractAssociatedIcon(ApplicationInfo.ExecutableFile))
			{
				if (appIcon != null)
				{
					// From comment at: http://www.infosysblogs.com/microsoft/2007/04/wpf_assigning_icon_to_image_co.html
					BitmapSource bitmap = Imaging.CreateBitmapSourceFromHIcon(appIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
					this.icon.Source = bitmap;
				}
			}

			this.Owner = owner;
			this.ShowDialog();
		}

		#endregion

		#region Private Methods

		private static void FinishLink(object sender, RoutedEventArgs e)
		{
			// Hyperlink doesn't have a Visited propertly like it did in WinForms, so we'll just set the color.
			// This loses the color change behavior on mouse hover, but I can live with that.
			// (I thought about just closing the about box instead.)
			if (sender is Control element)
			{
				element.Foreground = Brushes.Purple;
			}

			e.Handled = true;
		}

		#endregion

		#region Private Event Handlers

		[SuppressMessage("Usage", "CC0068:Unused Method", Justification = "Invoked via XAML reflection.")]
		private void EmailLink_Clicked(object sender, RoutedEventArgs e)
		{
			// UriBuilder always adds a '/' after the email address, which shows up in the opened
			// mail message's To field.  So we'll manually build a mailto-compatible Uri.
			string link = string.Format("mailto:bill@menees.com?subject={0} {1}", this.productName.Text, this.version.Text);
			if (WindowsUtility.ShellExecute(this, link))
			{
				FinishLink(sender, e);
			}

			e.Handled = true;
		}

		[SuppressMessage("Usage", "CC0068:Unused Method", Justification = "Invoked via XAML reflection.")]
		private void WebLink_Clicked(object sender, RoutedEventArgs e)
		{
			if (WindowsUtility.ShellExecute(this, "http://www.menees.com"))
			{
				FinishLink(sender, e);
			}

			e.Handled = true;
		}

		[SuppressMessage("Usage", "CC0068:Unused Method", Justification = "Invoked via XAML reflection.")]
		private async void ExtendedDialog_LoadedAsync(object sender, RoutedEventArgs e)
		{
			string repository = this.repository;
			Release? latest = await Task.Run(() => Release.FindGithubLatest(repository)).ConfigureAwait(true);
			if (latest != null && this.IsLoaded)
			{
				Version? appVersion = ReflectionUtility.GetVersion(this.callingAssembly);
				if (latest.Version > appVersion)
				{
					this.updateLink.Tag = latest;
					this.updateLink.Content = $"★ Version {latest.Version} update available!";
					this.updateLink.Visibility = Visibility.Visible;
				}
			}
		}

		[SuppressMessage("Usage", "CC0068:Unused Method", Justification = "Invoked via XAML reflection.")]
		private void UpdateLink_Clicked(object sender, RoutedEventArgs e)
		{
			if (this.updateLink.Tag is Release latest && WindowsUtility.ShellExecute(this, latest.HtmlUri.ToString()))
			{
				FinishLink(sender, e);
			}

			e.Handled = true;
		}

		#endregion
	}
}
