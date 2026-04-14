namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Runtime.CompilerServices;
	using System.Text;
	using Menees.Diagnostics;

	#endregion

	/// <summary>
	/// Provides several methods for checking preconditions and postconditions
	/// (i.e., design-by-contract).
	/// </summary>
	public static class Conditions
	{
		#region Public Methods

		/// <summary>
		/// Requires that the argument's state is valid (i.e., true).
		/// </summary>
		/// <param name="argState">The argument's state.</param>
		/// <param name="explanation">The name of the arg to put in the exception.</param>
		/// <exception cref="ArgumentException">If <paramref name="argState"/> is false.</exception>
		public static void RequireArgument([DoesNotReturnIf(false)] bool argState, string explanation)
		{
			RequireArgument(argState, explanation, null);
		}

		/// <summary>
		/// Requires that the named argument's state is valid (i.e., true).
		/// </summary>
		/// <param name="argState">The argument's state.</param>
		/// <param name="explanation">The explanation to put in the exception.</param>
		/// <param name="argName">The name of the arg to put in the exception.</param>
		/// <exception cref="ArgumentException">If <paramref name="argState"/> is false.</exception>
		public static void RequireArgument([DoesNotReturnIf(false)] bool argState, string explanation, string? argName)
		{
			if (!argState)
			{
				throw Exceptions.NewArgumentException(explanation, argName);
			}
		}

		/// <summary>
		/// Makes sure a collection is non-null and non-empty.
		/// </summary>
		/// <typeparam name="T">The type of item in the collection.</typeparam>
		/// <param name="arg">The collection to check.</param>
		/// <param name="argName">The name of the argument to put in the exception.</param>
		public static IEnumerable<T> RequireCollection<T>([NotNull] IEnumerable<T>? arg, [CallerArgumentExpression(nameof(arg))] string? argName = null)
		{
			RequireArgument(!CollectionUtility.IsNullOrEmpty(arg), "The collection must be non-empty.", argName);
			return arg;
		}

		/// <summary>
		/// Requires that a reference is non-null.
		/// </summary>
		/// <param name="reference">The reference to check.</param>
		/// <param name="argName">The arg name to put in the exception.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="reference"/> is null.</exception>
		[return: NotNull]
		public static T RequireReference<T>([NotNull] T? reference, [CallerArgumentExpression(nameof(reference))] string? argName = null)
		{
			// Note: This method doesn't use a "where T : class" constraint because we need to allow generic methods
			// and types to call Conditions.RequireReference even if they aren't constrained to just classes.
			if (reference == null)
			{
				throw Exceptions.NewArgumentNullException(argName!);
			}

			return reference;
		}

		/// <summary>
		/// Requires that the given state is valid (i.e., true).
		/// </summary>
		/// <param name="state">The state to check.</param>
		/// <param name="explanation">The explanation to put in the exception.</param>
		/// <exception cref="InvalidOperationException">If <paramref name="state"/> is false.</exception>
		public static void RequireState([DoesNotReturnIf(false)] bool state, string explanation)
		{
			if (!state)
			{
				throw Exceptions.NewInvalidOperationException(explanation);
			}
		}

		/// <summary>
		/// Requires that a string is non-null and non-empty.
		/// </summary>
		/// <param name="arg">The string to check.</param>
		/// <param name="argName">The name of the arg to put in the exception.</param>
		/// <exception cref="ArgumentException">If <paramref name="arg"/> is null or empty.</exception>
		public static string RequireString([NotNull] string? arg, [CallerArgumentExpression(nameof(arg))] string? argName = null)
		{
			RequireArgument(arg.IsNotEmpty(), "The string must be non-empty.", argName);
			return arg;
		}

		#endregion
	}
}
