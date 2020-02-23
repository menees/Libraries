namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Data.Entity.Design.PluralizationServices;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	#endregion

	public static partial class TextUtility
	{
		#region Public Methods

		/// <summary>
		/// Gets the plural form of the specified word.
		/// </summary>
		/// <param name="word">The word to make plural.</param>
		public static string MakePlural(string word)
		{
			PluralizationService service = CreatePluralizationService();
			string result = service.Pluralize(word);
			return result;
		}

		/// <summary>
		/// Gets the singular form of the specified word.
		/// </summary>
		/// <param name="word">The word to make singular.</param>
		public static string MakeSingular(string word)
		{
			PluralizationService service = CreatePluralizationService();
			string result = service.Singularize(word);
			return result;
		}

		#endregion

		#region Private Methods

		private static PluralizationService CreatePluralizationService()
		{
			// Entity Framework's pluralization service only works with English, which is all we
			// care about, but we need to make sure we pass it an English culture.
			// http://weblogs.asp.net/kencox/archive/2010/04/10/ef-4-s-pluralizationservice-class-a-singularly-impossible-plurality.aspx
			PluralizationService result = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-US"));
			return result;
		}

		#endregion
	}
}
