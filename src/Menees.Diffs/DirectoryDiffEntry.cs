namespace Menees.Diffs
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;

	#endregion

	public sealed class DirectoryDiffEntry
	{
		#region Private Data Members

		private readonly bool inA;
		private readonly bool inB;
		private readonly bool isFile;
		private readonly string name;
		private DirectoryDiffEntryCollection? subentries;

		#endregion

		#region Constructors

		internal DirectoryDiffEntry(string name, bool isFile, bool inA, bool inB, bool different)
		{
			this.name = name;
			this.isFile = isFile;
			this.inA = inA;
			this.inB = inB;
			this.Different = different;
		}

		#endregion

		#region Public Properties

		public bool Different { get; internal set; }

		public string? Error { get; internal set; }

		public bool InA => this.inA;

		public bool InB => this.inB;

		public bool IsFile => this.isFile;

		public string Name => this.name;

		public object? TagA { get; set; }

		public object? TagB { get; set; }

		public DirectoryDiffEntryCollection? Subentries
		{
			get
			{
				if (this.subentries == null && !this.isFile)
				{
					this.subentries = new DirectoryDiffEntryCollection();
				}

				return this.subentries;
			}
		}

		#endregion
	}
}
