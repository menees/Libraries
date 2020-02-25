namespace Menees.Diffs
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;

	#endregion

	public sealed class Addition : IAddCopy
	{
		#region Private Data Members

		private readonly byte[] bytes;

		#endregion

		#region Constructors

		internal Addition(byte[] bytes)
		{
			this.bytes = bytes;
		}

		#endregion

		#region Public Properties

		public bool IsAdd => true;

		#endregion

		#region Public Methods

		public byte[] GetBytes() => this.bytes;

		#endregion
	}
}
