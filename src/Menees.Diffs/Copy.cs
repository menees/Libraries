namespace Menees.Diffs
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;

	#endregion

	public sealed class Copy : IAddCopy
	{
		#region Constructors

		internal Copy(int baseOffset, int length)
		{
			this.BaseOffset = baseOffset;
			this.Length = length;
		}

		#endregion

		#region Public Properties

		public bool IsAdd => false;

		public int BaseOffset { get; }

		public int Length { get; }

		#endregion
	}
}
