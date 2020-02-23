namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;

	#endregion

	[SuppressMessage("", "CA1812", Justification = "Created via reflection using DebuggerTypeProxy.")]
	internal sealed class ReadOnlySetDebugView<T>
	{
		#region Private Data Members

		private readonly ISet<T> set;

		#endregion

		#region Constructors

		public ReadOnlySetDebugView(ReadOnlySet<T> set)
		{
			this.set = set ?? throw Exceptions.NewArgumentNullException(nameof(set));
		}

		#endregion

		#region Public Properties

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items
		{
			get
			{
				T[] array = new T[this.set.Count];
				this.set.CopyTo(array, 0);
				return array;
			}
		}

		#endregion
	}
}
