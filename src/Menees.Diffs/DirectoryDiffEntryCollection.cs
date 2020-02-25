namespace Menees.Diffs
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;

	#endregion

	public sealed class DirectoryDiffEntryCollection : ReadOnlyCollection<DirectoryDiffEntry>
	{
		#region Constructors

		internal DirectoryDiffEntryCollection()
			: base(new List<DirectoryDiffEntry>())
		{
		}

		#endregion

		#region Internal Methods

		internal void Add(DirectoryDiffEntry entry)
		{
			this.Items.Add(entry);
		}

		#endregion
	}
}
