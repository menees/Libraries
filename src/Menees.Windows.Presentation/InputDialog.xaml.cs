namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Shapes;

	#endregion

	internal partial class InputDialog : ExtendedDialog
	{
		#region Private Data Members

		private Func<string, string> validate;

		#endregion

		#region Constructors

		public InputDialog()
		{
			this.InitializeComponent();
		}

		#endregion

		#region Public Methods

		public string Execute(
			Window owner,
			string prompt,
			string defaultValue,
			int? maxLength,
			Func<string, string> validate)
		{
			this.prompt.Text = prompt;
			this.value.Text = defaultValue;
			this.validate = validate;
			if (maxLength != null)
			{
				this.value.MaxLength = maxLength.Value;
			}

			this.Owner = owner;
			if (owner == null)
			{
				// If there's no owner window, then this dialog should be centered on the screen.
				this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				this.ShowInTaskbar = true;
			}

			string result = null;
			if (this.ShowDialog() ?? false)
			{
				result = this.value.Text;
			}

			return result;
		}

		#endregion

		#region Private Event Handlers

		private void OKClicked(object sender, RoutedEventArgs e)
		{
			string error = this.validate?.Invoke(this.value.Text);

			if (string.IsNullOrEmpty(error))
			{
				this.DialogResult = true;
			}
			else
			{
				WindowsUtility.ShowError(this, error);
			}
		}

		#endregion
	}
}
