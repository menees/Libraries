namespace Menees.Windows.Forms
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;

	#endregion

	#region public FindMode

	/// <summary>
	/// The supported modes for <see cref="IFindTarget.Find"/>.
	/// </summary>
	public enum FindMode
	{
		/// <summary>
		/// Shows an <see cref="IFindDialog"/>.
		/// </summary>
		ShowDialog,

		/// <summary>
		/// Finds the next instance or shows an <see cref="IFindDialog"/> if there is no <see cref="FindData.Text"/>.
		/// </summary>
		FindNext,

		/// <summary>
		/// Finds the previous instance or shows an <see cref="IFindDialog"/> if there is no <see cref="FindData.Text"/>.
		/// </summary>
		FindPrevious,
	}

	#endregion

	#region public ListViewColumnType

	/// <summary>
	/// Defines the supported column types used in sorting.
	/// </summary>
	public enum ListViewColumnType
	{
#pragma warning disable CA1720 // Identifier contains type name
		/// <summary>
		/// A string column.
		/// </summary>
		String,
#pragma warning restore CA1720 // Identifier contains type name

		/// <summary>
		/// A numeric column.  This can be an integer or floating-point type.
		/// </summary>
		Number,

		/// <summary>
		/// A date/time column.
		/// </summary>
		DateTime,
	}

	#endregion
}
