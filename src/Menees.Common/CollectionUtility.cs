namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;

	#endregion

	/// <summary>
	/// Methods for collection handling.
	/// </summary>
	public static partial class CollectionUtility
	{
		#region Public Methods

		/// <summary>
		/// Adds a <paramref name="source"/> collection into a <paramref name="target"/> collection.
		/// </summary>
		/// <typeparam name="T">The type of item to add.</typeparam>
		/// <param name="target">The collection to add the items to.</param>
		/// <param name="source">The collection to get the items from.</param>
		/// <remarks>
		/// The adding behavior depends on the <paramref name="target"/> collection's <see cref="ICollection{T}.Add"/>
		/// implemenation.  For example, if <paramref name="target"/> is a generic List, then this could add duplicates.
		/// However, if <paramref name="target"/> is a generic HashSet, then this won't add duplicates.
		/// </remarks>
		public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
		{
			Conditions.RequireReference(target, nameof(target));
			Conditions.RequireReference(source, nameof(source));

			foreach (T item in source)
			{
				target.Add(item);
			}
		}

		/// <summary>
		/// Returns a collection containing only the specified value.
		/// </summary>
		/// <typeparam name="T">The type of value to contain.</typeparam>
		/// <param name="value">The value to include in the returned collection.</param>
		/// <returns>A collection containing <paramref name="value"/>.</returns>
		public static IEnumerable<T> AsEnumerable<T>(T value)
		{
			// This is the fastest guaranteed-safe way to return a collection from a single value.
			// This is faster than returning Enumerable.Repeat(value, 1), and it is safer than
			// returning new[] { value } because the result can't be modified even after casting.
			// For background info see: http://stackoverflow.com/questions/1577822/passing-a-single-item-as-ienumerablet
			// and http://geekswithblogs.net/BlackRabbitCoder/archive/2011/12/08/c-fundamentals-returning-zero-or-one-item-as-ienumerablelttgt.aspx
			yield return value;
		}

		/// <summary>
		/// Gets a read-only dictionary.
		/// </summary>
		/// <param name="dictionary">The dictionary to wrap or return.  This must be non-null.</param>
		/// <returns>If the original <paramref name="dictionary"/> is already read-only, this method
		/// returns it as is.  If it is not read-only, this method returns a read-only wrapper around it.</returns>
		public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
			where TKey : notnull
		{
			Conditions.RequireReference(dictionary, nameof(dictionary));

			IDictionary<TKey, TValue> result = dictionary;

			if (result != null && !result.IsReadOnly)
			{
				result = new ReadOnlyDictionary<TKey, TValue>(dictionary);
			}

			return result!;
		}

		/// <summary>
		/// Gets a read-only set.
		/// </summary>
		/// <param name="value">The set to wrap or return.  This must be non-null.</param>
		/// <returns>If the original <paramref name="value"/> is already read-only, this method
		/// returns it as is.  If it is not read-only, this method returns a read-only wrapper around it.</returns>
		public static ISet<T> AsReadOnly<T>(this ISet<T> value)
		{
			Conditions.RequireReference(value, nameof(value));

			ISet<T> result = value;

			if (result != null && !result.IsReadOnly)
			{
				result = new ReadOnlySet<T>(value);
			}

			return result!;
		}

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

		/// <summary>
		/// Gets whether the specified collection is null or empty.
		/// </summary>
		/// <typeparam name="T">The type of element in the collection.</typeparam>
		/// <param name="collection">The collection to check.</param>
		/// <returns>True if <paramref name="collection"/> is null or doesn't contain any items.</returns>
		public static bool IsNullOrEmpty<T>([NotNullWhen(false)] IEnumerable<T>? collection)
		{
			bool result = collection == null || !collection.Any();
			return result;
		}

		#endregion
	}
}
