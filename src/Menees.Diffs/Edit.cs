namespace Menees.Diffs
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;

	#endregion

	[DebuggerDisplay("Type = {EditType}, StartA = {StartA}, StartB = {StartB}, Length = {Length}")]
	public sealed class Edit
	{
		#region Private Data Members

		private readonly EditType editType;
		private readonly int length;
		private int startA;	// Where to Delete, Insert, or Change in the "A" sequence
		private int startB;	// Where to Insert or Change in the "B" sequence

		#endregion

		#region Constructors

		internal Edit(EditType editType, int startA, int startB, int length)
		{
			this.editType = editType;
			this.startA = startA;
			this.startB = startB;
			this.length = length;
		}

		#endregion

		#region Public Properties

		public int Length => this.length;

		public int StartA => this.startA;

		public int StartB => this.startB;

		public EditType EditType => this.editType;

		#endregion

		#region Public Methods

		public void Offset(int offsetA, int offsetB)
		{
			this.startA += offsetA;
			this.startB += offsetB;
		}

		#endregion
	}
}
