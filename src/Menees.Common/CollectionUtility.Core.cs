namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	#endregion

	public static partial class CollectionUtility
	{
		#region Public Methods

		/// <summary>
		/// Returns an empty array of the specified type.
		/// </summary>
		/// <typeparam name="TResult">The type of elements in the returned array.</typeparam>
		/// <returns>An empty array of the specified type.</returns>
		/// <remarks>
		/// This method caches an empty array of each type requested, and it is more efficient
		/// than creating empty array instances repeatedly.
		/// <para/>
		/// This method is similar to <see cref="Enumerable.Empty{TResult}"/> but this returns
		/// an array instead of just an <see cref="IEnumerable{TResult}"/>.
		/// </remarks>
		public static TResult[] EmptyArray<TResult>() => Array.Empty<TResult>();

		#endregion
	}
}
