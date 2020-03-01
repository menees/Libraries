namespace Menees.Diffs.Controls
{
	#region Using Directives

	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;
	using Menees.Windows.Forms;

	#endregion

	internal sealed partial class GoToDialog : ExtendedForm
	{
		#region Constructors

		public GoToDialog()
		{
			this.InitializeComponent();
		}

		#endregion

		#region Public Methods

		public bool Execute(IWin32Window owner, int maxLineNumber, out int line)
		{
			line = 0;
			bool result = false;

			this.lblLineNumber.Text = string.Format("&Line Number (1-{0}):", maxLineNumber);
			this.edtLineNumber.Maximum = maxLineNumber;
			if (this.ShowDialog(owner) == DialogResult.OK)
			{
				line = (int)this.edtLineNumber.Value;
				result = true;
			}

			return result;
		}

		#endregion

		#region Private Event Handlers

		private void GoToDialog_Shown(object sender, EventArgs e)
		{
			this.edtLineNumber.Select(0, int.MaxValue);
		}

		#endregion
	}
}
