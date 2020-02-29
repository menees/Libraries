namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;

	#endregion

	public static partial class TextUtility
	{
		#region Public Methods

		/// <summary>
		/// Gets the plural form of the specified word.
		/// </summary>
		/// <param name="word">The word to make plural.</param>
		public static string MakePlural(string word) => Pluralizer.MakePlural(word);

		#endregion

		#region Private Types

		private static class Pluralizer
		{
			#region Private Data Members

			private static readonly Dictionary<string, string> SpecialCases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				// These should all be in lowercase (for PreserveCase logic).
				{ "deer", "deer" },
				{ "tooth", "teeth" },
			};

			#endregion

			#region Public Methods

			// I don't want to pull in a whole other library for this. This code works fine for the cases I know of.
			// https://www.nuget.org/packages/Pluralize.NET.Core/
			// https://stackoverflow.com/a/47410837
			// https://github.com/Microsoft/referencesource/blob/master/System.Data.Entity.Design/System/Data/Entity/Design/PluralizationService/...
			// ... EnglishPluralizationService.cs
			[SuppressMessage("Design", "MEN010:Avoid magic numbers", Justification = "The length values are clear in context.")]
			public static string MakePlural(string word)
			{
				string result;

				int length = word?.Length ?? 0;

				if (length == 0)
				{
					result = word;
				}
				else if (SpecialCases.TryGetValue(word, out string plural))
				{
					result = plural;
				}
				else if (Equals(Right(word, 3), "rix"))
				{
					result = Left(word, length - 1) + "ces";
				}
				else if (In(Right(word, 3), "dex", "tex"))
				{
					result = Left(word, length - 2) + "ices";
				}
				else if (Equals(Right(word, 3), "man"))
				{
					result = Left(word, length - 2) + "en";
				}
				else if (Equals(Right(word, 4), "ouse") && In(Left(word, 1), "l", "m"))
				{
					result = Left(word, length - 4) + "ice";
				}
				else if (Equals(Right(word, 5), "goose"))
				{
					result = Left(word, length - 4) + "eese";
				}
				else if (Equals(Right(word, 4), "ocus"))
				{
					result = Left(word, length - 2) + "i";
				}
				else if (In(Right(word, 2), "ax", "ex", "ix", "ox", "ux", "ch", "sh", "as", "ss", "us"))
				{
					result = word + "es";
				}
				else if (In(Right(word, 1), "c", "z"))
				{
					result = word + "es";
				}
				else if (In(Right(word, 2), "ay", "ey", "iy", "oy", "uy", "ae", "ee", "ie", "oe", "ue"))
				{
					result = word + "s";
				}
				else if (Equals(Right(word, 1), "y"))
				{
					result = Left(word, length - 1) + "ies";
				}
				else if (!In(Right(word, 1), "s", "x"))
				{
					result = word + "s";
				}
				else
				{
					result = word;
				}

				result = PreserveCase(word, result);
				return result;
			}

			#endregion

			#region Private Methods

			private static bool Equals(string left, string right) => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

			private static bool In(string left, params string[] right) => right.Any(value => Equals(left, value));

			private static string Left(string value, int count) => count < value.Length ? value.Substring(0, count) : value;

			private static string Right(string value, int count) => count < value.Length ? value.Substring(value.Length - count) : value;

			#endregion
		}

		#endregion
	}
}
