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

			// I don't want to pull in a whole other library for this. This code works fine for the cases I need it for,
			// which are item types on a PC's file system. Here are some heavier weight alternatives:
			// https://www.nuget.org/packages/Pluralize.NET.Core/
			// https://stackoverflow.com/a/47410837
			// http://users.monash.edu/~damian/papers/HTML/Plurals.html
			// https://github.com/Microsoft/referencesource/blob/master/System.Data.Entity.Design/...
			//  ... System/Data/Entity/Design/PluralizationService/EnglishPluralizationService.cs
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
				else if (IsSuffix(word, "ss", "ch"))
				{
					result = word + "es";
				}
				else if (IsSuffix(word, "rix", "dex"))
				{
					result = Prefix(word, -2) + "ices";
				}
				else if (IsSuffix(word, "man"))
				{
					result = Prefix(word, -2) + "en";
				}
				else if (IsSuffix(word, "ay", "ey", "iy", "oy", "uy"))
				{
					result = word + "s";
				}
				else if (IsSuffix(word, "y"))
				{
					result = Prefix(word, -1) + "ies";
				}
				else if (!IsSuffix(word, "s"))
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

			private static string Prefix(string value, int count) => value.Substring(0, count < 0 ? (value.Length + count) : count);

			private static bool IsSuffix(string value, params string[] options)
				=> options.Any(option => value.EndsWith(option, StringComparison.OrdinalIgnoreCase));

			#endregion
		}

		#endregion
	}
}
