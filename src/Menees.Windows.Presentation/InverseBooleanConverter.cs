namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Windows.Data;

	#endregion

	/// <summary>
	/// A value converter that inverts a boolean value.
	/// </summary>
	/// <remarks>
	/// This is useful when you need to disable a control when something else is enabled.
	/// </remarks>
	[ValueConversion(typeof(bool), typeof(bool))]
	public class InverseBooleanConverter : IValueConverter
	{
		#region Public Methods

		/// <summary>
		/// Converts <paramref name="value"/> to a Boolean and inverts it.
		/// </summary>
		/// <param name="value">Any value supported by <see cref="System.Convert.ToBoolean(object)"/>.</param>
		/// <param name="targetType">Must be System.Boolean.</param>
		/// <param name="parameter">The parameter is ignored.</param>
		/// <param name="culture">The culture is ignored.</param>
		/// <returns>The inverse of <paramref name="value"/> as a Boolean.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// From: http://stackoverflow.com/questions/1039636/how-to-bind-inverse-boolean-properties-in-wpf
			if (targetType != typeof(bool))
			{
				throw Exceptions.NewInvalidOperationException("The target type must be a Boolean.");
			}

			bool result = !System.Convert.ToBoolean(value);
			return result;
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
