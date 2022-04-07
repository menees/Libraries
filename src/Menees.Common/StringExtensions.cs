namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Text;

	#endregion

	/// <summary>
	/// Adds extension methods to <see cref="string"/>.
	/// </summary>
	/// <remarks>
	/// These methods are properly annotated for nullable analysis, unlike string.IsNullOrEmpty in netstandard2.0 and net48 builds.
	/// </remarks>
	public static class StringExtensions
	{
		#region Public Methods

		/// <summary>
		/// Shortcut for <see cref="string.IsNullOrWhiteSpace"/>. Blank means "null, empty, or whitespace".
		/// </summary>
		[Obsolete("Use IsWhiteSpace() instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public static bool IsBlank([NotNullWhen(false)] this string? text) => text.IsWhiteSpace();

		/// <summary>
		/// Shortcut for NOT <see cref="string.IsNullOrWhiteSpace"/>. Blank means "null, empty, or whitespace".
		/// </summary>
		[Obsolete("Use IsNotWhiteSpace() instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public static bool IsNotBlank([NotNullWhen(true)] this string? text) => text.IsNotWhiteSpace();

		/// <summary>
		/// Shortcut for <see cref="string.IsNullOrEmpty"/>.
		/// </summary>
		public static bool IsEmpty([NotNullWhen(false)] this string? text) => string.IsNullOrEmpty(text);

		/// <summary>
		/// Shortcut for NOT <see cref="string.IsNullOrEmpty"/>.
		/// </summary>
		public static bool IsNotEmpty([NotNullWhen(true)] this string? text) => !string.IsNullOrEmpty(text);

		/// <summary>
		/// Shortcut for <see cref="string.IsNullOrWhiteSpace"/>.
		/// </summary>
		public static bool IsWhiteSpace([NotNullWhen(false)] this string? text) => string.IsNullOrWhiteSpace(text);

		/// <summary>
		/// Shortcut for NOT <see cref="string.IsNullOrWhiteSpace"/>.
		/// </summary>
		public static bool IsNotWhiteSpace([NotNullWhen(true)] this string? text) => !string.IsNullOrWhiteSpace(text);

		#endregion
	}
}
