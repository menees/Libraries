namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Text;

	#endregion

	/// <summary>
	/// Adds extension methods to <see cref="string"/>.
	/// </summary>
	/// <remarks>
	/// These methods are properly annotated for nullable analysis, unlike string.IsNullOrEmpty in netstandard2.0 builds.
	/// </remarks>
	public static class StringExtensions
	{
		#region Public Methods

		/// <summary>
		/// Shortcut for <see cref="string.IsNullOrWhiteSpace"/>. Blank means "null, empty, or whitespace".
		/// </summary>
		public static bool IsBlank([NotNullWhen(false)] this string? text) => string.IsNullOrWhiteSpace(text);

		/// <summary>
		/// Shortcut for NOT <see cref="string.IsNullOrWhiteSpace"/>. Blank means "null, empty, or whitespace".
		/// </summary>
		public static bool IsNotBlank([NotNullWhen(true)] this string? text) => !string.IsNullOrWhiteSpace(text);

		/// <summary>
		/// Shortcut for <see cref="string.IsNullOrEmpty"/>.
		/// </summary>
		public static bool IsEmpty([NotNullWhen(false)] this string? text) => string.IsNullOrEmpty(text);

		/// <summary>
		/// Shortcut for NOT <see cref="string.IsNullOrEmpty"/>.
		/// </summary>
		public static bool IsNotEmpty([NotNullWhen(true)] this string? text) => !string.IsNullOrEmpty(text);

		#endregion
	}
}
