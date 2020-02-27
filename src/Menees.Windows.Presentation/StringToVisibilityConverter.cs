namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Data;

	#endregion

	/// <summary>
	/// Converts null, empty, or non-empty string values into Visibility values.
	/// </summary>
	public class StringToVisibilityConverter : IValueConverter
	{
		#region Public Methods

		/// <summary>
		/// Converts a string value (null, empty, or non-empty) into a Visibility.
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string text = value as string;
			Visibility result = string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
			return result;
		}

		/// <summary>
		/// Returns UnsetValue.
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => DependencyProperty.UnsetValue;

		#endregion
	}
}
