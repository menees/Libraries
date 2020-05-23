namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using System.Windows.Controls;

	#endregion

	/// <summary>
	/// Validates that a text value is non-empty and non-whitespace.
	/// </summary>
	/// <remarks>
	/// This can be used in Binding.ValidationRules to ensure that a text value is non-empty before a target's
	/// "edit mode" is exited. An invalid result will typically put a red border around the TextBox, and it will keep
	/// focus on that TextBox until the user enters a valid value or cancels.
	/// <para/>
	/// This is typically only necessary for string properties because for other data types WPF will apply
	/// a string-to-X converter before leaving edit mode.
	/// </remarks>
	public sealed class NonEmptyTextValidationRule : ValidationRule
	{
		#region Public Properties

		/// <summary>
		/// Gets or sets the error message to return from <see cref="Validate"/> for an invalid result.
		/// </summary>
		public string ErrorMessage { get; set; } = "Please enter a non-empty, non-whitespace value.";

		#endregion

		#region Public Methods

		/// <summary>
		/// Validates that the input value is a non-empty, non-whitespace string.
		/// </summary>
		/// <param name="value">The value to validate.</param>
		/// <param name="cultureInfo">Ignored.</param>
		/// <returns>Whether <paramref name="value"/> is non-empty and non-whitespace.</returns>
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			ValidationResult result = ValidationResult.ValidResult;

			if (value == null || !(value is string text) || string.IsNullOrWhiteSpace(text))
			{
				result = new ValidationResult(false, this.ErrorMessage);
			}

			return result;
		}

		#endregion
	}
}
