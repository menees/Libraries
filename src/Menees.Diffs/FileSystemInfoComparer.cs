namespace Menees.Diffs
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;

	#endregion

	internal class FileSystemInfoComparer : IComparer<FileSystemInfo>
	{
		#region Private Data Members

		private static readonly FileSystemInfoComparer CurrentCultureComparer =
			new FileSystemInfoComparer(StringComparison.CurrentCulture);

		private static readonly FileSystemInfoComparer CurrentCultureIgnoreCaseComparer =
			new FileSystemInfoComparer(StringComparison.CurrentCultureIgnoreCase);

		private static readonly FileSystemInfoComparer InvariantCultureComparer =
			new FileSystemInfoComparer(StringComparison.InvariantCulture);

		private static readonly FileSystemInfoComparer InvariantCultureIgnoreCaseComparer =
			new FileSystemInfoComparer(StringComparison.InvariantCultureIgnoreCase);

		private static readonly FileSystemInfoComparer OrdinalComparer =
			new FileSystemInfoComparer(StringComparison.Ordinal);

		private static readonly FileSystemInfoComparer OrdinalIgnoreCaseComparer =
			new FileSystemInfoComparer(StringComparison.OrdinalIgnoreCase);

		#endregion

		#region Constructors

		private FileSystemInfoComparer(StringComparison comparison)
		{
			this.Comparison = comparison;
		}

		#endregion

		#region Public Properties

		public StringComparison Comparison { get; }

		#endregion

		#region Public Methods

		public static FileSystemInfoComparer Get(StringComparison comparison)
			=> comparison switch
			{
				StringComparison.CurrentCulture => CurrentCultureComparer,
				StringComparison.CurrentCultureIgnoreCase => CurrentCultureIgnoreCaseComparer,
				StringComparison.InvariantCulture => InvariantCultureComparer,
				StringComparison.InvariantCultureIgnoreCase => InvariantCultureIgnoreCaseComparer,
				StringComparison.Ordinal => OrdinalComparer,
				StringComparison.OrdinalIgnoreCase => OrdinalIgnoreCaseComparer,
				_ => CurrentCultureComparer,
			};

		public int Compare(FileSystemInfo x, FileSystemInfo y) => string.Compare(x.Name, y.Name, this.Comparison);

		#endregion
	}
}
