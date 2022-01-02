namespace Menees.Diffs
{
	#region Using Directives

	using System;
	using System.Collections.Generic;

	#endregion

	/// <summary>
	/// This class uses the MyersDiff helper class to difference two
	/// string lists.  It hashes each string in both lists and then
	/// differences the resulting integer arrays.
	/// </summary>
	public sealed class TextDiff
	{
		#region Private Data Members

		private readonly bool supportChangeEditType;
		private readonly StringHasher hasher;

		#endregion

		#region Constructors

		public TextDiff(HashType hashType, bool ignoreCase, bool ignoreOuterWhiteSpace)
			: this(hashType, ignoreCase, ignoreOuterWhiteSpace, 0, true)
		{
		}

		public TextDiff(HashType hashType, bool ignoreCase, bool ignoreOuterWhiteSpace, int leadingCharactersToIgnore, bool supportChangeEditType)
		{
			this.hasher = new StringHasher(hashType, ignoreCase, ignoreOuterWhiteSpace, leadingCharactersToIgnore);
			this.supportChangeEditType = supportChangeEditType;
		}

		#endregion

		#region Public Methods

		public EditScript Execute(IList<string> listA, IList<string> listB)
		{
			int[] hashA = this.HashStringList(listA);
			int[] hashB = this.HashStringList(listB);

			MyersDiff<int> diff = new(hashA, hashB, this.supportChangeEditType);
			EditScript result = diff.Execute();
			return result;
		}

		#endregion

		#region Private Methods

		private int[] HashStringList(IList<string> lines)
		{
			int numLines = lines.Count;
			int[] result = new int[numLines];

			for (int i = 0; i < numLines; i++)
			{
				result[i] = this.hasher.GetHashCode(lines[i]);
			}

			return result;
		}

		#endregion
	}
}
