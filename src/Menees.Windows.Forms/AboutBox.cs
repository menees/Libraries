namespace Menees.Windows.Forms
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Data;
	using System.Drawing;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Windows.Forms;
	using Menees.Diagnostics;
	using Menees.Shell;

	#endregion

	internal partial class AboutBox : ExtendedForm
	{
		#region Private Data Members

		private readonly Assembly callingAssembly;
		private readonly string repository;

		#endregion

		#region Constructors

		public AboutBox(Assembly callingAssembly, string repository)
		{
			this.InitializeComponent();

			this.callingAssembly = callingAssembly;
			this.repository = repository ?? ApplicationInfo.ApplicationName.Replace(" ", string.Empty);

			string applicationName = ApplicationInfo.ApplicationName;
			this.Text = "About " + applicationName;
			this.productName.Text = applicationName;
			this.version.Text = ShellUtility.GetVersionInfo(callingAssembly);
			this.copyright.Text = ShellUtility.GetCopyrightInfo(callingAssembly);
		}

		#endregion

		#region Public Methods

		public void Execute(IWin32Window owner)
		{
			bool useDesktopAsOwner = owner == null;

			if (owner is Control control)
			{
				Form form = control.FindForm();
				if (form != null)
				{
					this.icon.Image = form.Icon.ToBitmap();

					// This is important for tray icon apps where the main form may be hidden.
					useDesktopAsOwner = !form.Visible;
				}
				else
				{
					useDesktopAsOwner = true;
				}
			}
			else if (owner != null)
			{
				// We should only get here for a WPF app or an unmanaged app.
				// From WinUser.h
				const int WM_GETICON = 0x007F;
				IntPtr iconBig = (IntPtr)1;
				IntPtr iconHandle = NativeMethods.SendMessage(new HandleRef(null, owner.Handle), WM_GETICON, iconBig, IntPtr.Zero);
				if (iconHandle != IntPtr.Zero)
				{
					using (Icon ownerIcon = Icon.FromHandle(iconHandle))
					{
						if (ownerIcon != null)
						{
							this.icon.Image = ownerIcon.ToBitmap();
						}
					}
				}
			}

			if (useDesktopAsOwner)
			{
				// If there's no owner window, then this dialog should be centered on the screen.
				this.StartPosition = FormStartPosition.CenterScreen;
				this.ShowInTaskbar = true;
			}

			if (this.icon.Image == null)
			{
				using (Icon appIcon = Icon.ExtractAssociatedIcon(ApplicationInfo.ExecutableFile))
				{
					this.icon.Image = appIcon.ToBitmap();
				}
			}

			this.ShowDialog(owner);
		}

		#endregion

		#region Private Methods

		private void VisitLink(LinkLabel linkLabel, string linkUrl = null)
		{
			if (WindowsUtility.ShellExecute(this, linkUrl ?? linkLabel.Text))
			{
				linkLabel.Links[0].Visited = true;
			}
		}

		private void WebLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.VisitLink(this.webLink);
		}

		private void EmailLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// UriBuilder always adds a '/' after the email address, which shows up in the opened
			// mail message's To field.  So we'll manually build a mailto-compatible Uri.
			string link = string.Format("mailto:{0}?subject={1} {2}", this.emailLink.Text, this.productName.Text, this.version.Text);
			this.VisitLink(this.emailLink, link);
		}

		private void AboutBox_Shown(object sender, EventArgs e)
		{
			this.updateChecker.RunWorkerAsync();
		}

		private void UpdateChecker_DoWork(object sender, DoWorkEventArgs e)
		{
			Release latest = Release.FindGithubLatest(this.repository);
			if (latest != null)
			{
				Version appVersion = ReflectionUtility.GetVersion(this.callingAssembly);
				if (latest.Version > appVersion)
				{
					e.Result = latest;
				}
			}
		}

		private void UpdateChecker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error == null && e.Result is Release latest && !this.updateLink.Disposing && !this.Disposing)
			{
				this.updateLink.Tag = latest;
				this.updateLink.Text = $"★ Version {latest.Version} update available!";
				this.updateLink.Visible = true;
			}
		}

		private void UpdateLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (this.updateLink.Tag is Release latest)
			{
				this.VisitLink(this.updateLink, latest.HtmlUri.ToString());
			}
		}

		#endregion
	}
}
