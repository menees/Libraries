namespace Menees.Diffs
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.IO;

	#endregion

	public sealed class DirectoryDiffFileFilter
	{
		#region Private Data Members

		private readonly string concatenatedFilters;
		private readonly string[] individualFilters;
		private readonly bool include;
		private readonly FileSystemInfoComparer comparer;

		#endregion

		#region Constructors

		public DirectoryDiffFileFilter(string filters, bool include)
			: this(filters, include, DirectoryDiff.DefaultNameComparison)
		{
		}

		public DirectoryDiffFileFilter(string filters, bool include, StringComparison nameComparison)
		{
			Conditions.RequireString(filters, nameof(filters));
			this.concatenatedFilters = filters;
			this.include = include;
			this.individualFilters = filters.Split(';');
			for (int i = 0; i < this.individualFilters.Length; i++)
			{
				this.individualFilters[i] = this.individualFilters[i].Trim();
			}

			this.comparer = FileSystemInfoComparer.Get(nameComparison);
		}

		#endregion

		#region Public Properties

		public string Filters => this.concatenatedFilters;

		public bool Include => this.include;

		#endregion

		#region Public Methods

		public FileInfo[] Filter(DirectoryInfo directory)
		{
			// Get all the files that match the filters
			List<FileInfo> files = new();
			foreach (string filter in this.individualFilters)
			{
				FileInfo[] filterFiles = directory.GetFiles(filter);
				files.AddRange(filterFiles);
			}

			// Sort them
			files.Sort(this.comparer);

			// Throw out duplicates
			FileInfo previousFile = null;
			for (int i = 0; i < files.Count; /*Incremented in the loop*/)
			{
				FileInfo currentFile = files[i];
				if (previousFile != null && this.comparer.Compare(currentFile, previousFile) == 0)
				{
					files.RemoveAt(i);

					// Don't increment i;
				}
				else
				{
					previousFile = currentFile;
					i++;
				}
			}

			// Exclude these files if necessary
			FileInfo[] result;
			if (this.include)
			{
				result = files.ToArray();
			}
			else
			{
				FileInfo[] allFiles = directory.GetFiles();
				Array.Sort(allFiles, this.comparer);

				List<FileInfo> filesToInclude = new();
				int numExcludes = files.Count;
				int numTotal = allFiles.Length;
				int e = 0;
				for (int a = 0; a < numTotal; a++)
				{
					int compareResult = -1;
					FileInfo fileA = allFiles[a];
					if (e < numExcludes)
					{
						FileInfo fileE = files[e];
						compareResult = this.comparer.Compare(fileA, fileE);
					}

					if (compareResult == 0)
					{
						// Don't put this match in the results.
						e++;
					}
					else
					{
						filesToInclude.Add(fileA);
					}
				}

				result = filesToInclude.ToArray();
			}

			return result;
		}

		#endregion
	}
}
