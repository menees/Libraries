namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using System.Windows.Input;

	#endregion

	/// <summary>
	/// A disposable object that can be used to specify a "wait" cursor during long operations.
	/// </summary>
	/// <devnote>
	/// Based on WinForms WaitCursor and http://stackoverflow.com/questions/3480966/display-hourglass-when-application-is-busy.
	/// </devnote>
	public sealed class WaitCursor : IDisposable
	{
		#region Private Data Members

		private readonly Cursor previous;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new instance that changes the application's cursor to a wait cursor.
		/// </summary>
		public WaitCursor()
		{
			// Save the previous cursor so Dispose can restore it.  This allows us to work
			// correctly even if WaitCursor instances are nested.
			this.previous = Mouse.OverrideCursor;
			Apply();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Changes the application's cursor to a wait cursor.
		/// </summary>
		public static void Apply()
		{
			Mouse.OverrideCursor = Cursors.Wait;
		}

		/// <summary>
		/// Returns the application's cursor to its original state.
		/// </summary>
		public void Dispose()
		{
			Mouse.OverrideCursor = this.previous;
		}

		#endregion
	}
}
