namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
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
		/// Gets a read-only dictionary.
		/// </summary>
		/// <param name="dictionary">The dictionary to wrap or return.  This must be non-null.</param>
		/// <returns>If the original <paramref name="dictionary"/> is already read-only, this method
		/// returns it as is.  If it is not read-only, this method returns a read-only wrapper around it.</returns>
		public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
		{
			Conditions.RequireReference(dictionary, nameof(dictionary));

			IDictionary<TKey, TValue> result = dictionary;

			if (result != null && !result.IsReadOnly)
			{
				result = new ReadOnlyDictionary<TKey, TValue>(dictionary);
			}

			return result;
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

			return result;
		}

		#endregion
	}
}
