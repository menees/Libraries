namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using Menees.Diagnostics;

	#endregion

	/// <summary>
	/// Helper methods for creating and logging new exceptions.
	/// </summary>
	public static class Exceptions
	{
		#region Public Methods

		/// <summary>
		/// Creates and logs a new ArgumentException with the specified message.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <returns>A new exception instance.</returns>
		public static ArgumentException NewArgumentException(string message) => NewArgumentException(message, null, null);

		/// <summary>
		/// Creates and logs a new ArgumentException with the specified message and argument name.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="argName">The argument name.</param>
		/// <returns>A new exception instance.</returns>
		public static ArgumentException NewArgumentException(string message, string argName) => NewArgumentException(message, argName, null);

		/// <summary>
		/// Creates and logs a new ArgumentException with the specified message and context properties.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="contextProperties">Context information to log with the exception.</param>
		/// <returns>A new exception instance.</returns>
		public static ArgumentException NewArgumentException(string message, IDictionary<string, object> contextProperties)
			=> NewArgumentException(message, null, contextProperties);

		/// <summary>
		/// Creates and logs a new ArgumentException with the specified message, argument name, and context properties.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="argName">The argument name.</param>
		/// <param name="contextProperties">Context information to log with the exception.</param>
		/// <returns>A new exception instance.</returns>
		public static ArgumentException NewArgumentException(string message, string argName, IDictionary<string, object> contextProperties)
			=> Log(new ArgumentException(message, argName), contextProperties);

		/// <summary>
		/// Creates and logs a new ArgumentNullException.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <returns>A new exception instance.</returns>
		public static ArgumentNullException NewArgumentNullException(string argName) => Log(new ArgumentNullException(argName));

		/// <summary>
		/// Creates and logs a new ArgumentNullException with the specified context properties.
		/// </summary>
		/// <param name="argName">The argument name.</param>
		/// <param name="contextProperties">Context information to log with the exception.</param>
		/// <returns>A new exception instance.</returns>
		public static ArgumentNullException NewArgumentNullException(string argName, IDictionary<string, object> contextProperties)
			=> Log(new ArgumentNullException(argName), contextProperties);

		/// <summary>
		/// Logs the given exception as an error.
		/// </summary>
		/// <typeparam name="T">The type of exception to log and return.</typeparam>
		/// <param name="ex">The exception to log.
		/// <para/>Note: The <see cref="Exception.Data"/> dictionary
		/// may get modified for this exception.</param>
		/// <returns>The exception passed in as <paramref name="ex"/>.</returns>
		/// <remarks>
		/// See <see cref="Log&lt;T&gt;(T, Type)"/> for an example and more information.
		/// </remarks>
		public static T Log<T>(T ex)
			where T : Exception => Log(ex, null, null);

		/// <summary>
		/// Logs the given exception as an error.
		/// </summary>
		/// <typeparam name="T">The type of exception to log and return.</typeparam>
		/// <param name="ex">The exception to log.
		/// <para/>Note: The <see cref="Exception.Data"/> dictionary
		/// may get modified for this exception.</param>
		/// <param name="category">The configuration category, which is typically the caller's type.</param>
		/// <returns>The exception passed in as <paramref name="ex"/>.</returns>
		/// <remarks>
		/// Other methods in this class should be used if you're creating one of the following "typical"
		/// exceptions:
		/// <list type="table">
		///     <listheader>
		///         <term>Exception Type</term>
		///         <description>Method</description>
		///     </listheader>
		///     <item>
		///         <term>ArgumentException</term>
		///         <description><see cref="NewArgumentException(string, string)"/></description>
		///     </item>
		///     <item>
		///         <term>ArgumentNullException</term>
		///         <description><see cref="NewArgumentNullException(string)"/></description>
		///     </item>
		///     <item>
		///         <term>InvalidOperationException</term>
		///         <description><see cref="NewInvalidOperationException(string)"/></description>
		///     </item>
		/// </list>
		/// </remarks>
		/// <example>
		/// Using Exceptions.Log with a custom exception type.
		/// <code>
		/// if (badFormat)
		/// {
		///     throw Exceptions.Log(new FormatException(Properties.Resources.MyClass_BadFormat));
		/// }
		/// </code>
		/// </example>
		public static T Log<T>(T ex, Type category)
			where T : Exception => Log(ex, category, null);

		/// <summary>
		/// Logs the given exception as an error.
		/// </summary>
		/// <typeparam name="T">The type of exception to log and return.</typeparam>
		/// <param name="ex">The exception to log.
		/// <param name="contextProperties">Context information to log with the exception.</param>
		/// <para/>Note: The <see cref="Exception.Data"/> dictionary
		/// may get modified for this exception.</param>
		/// <returns>The exception passed in as <paramref name="ex"/>.</returns>
		/// <remarks>
		/// See <see cref="Log&lt;T&gt;(T, Type)"/> for an example and more information.
		/// </remarks>
		public static T Log<T>(T ex, IDictionary<string, object> contextProperties)
			where T : Exception => Log(ex, null, contextProperties);

		/// <summary>
		/// Logs the given exception as an error.
		/// </summary>
		/// <typeparam name="T">The type of exception to log and return.</typeparam>
		/// <param name="ex">The exception to log.
		/// <param name="category">The configuration category, which is typically the caller's type.</param>
		/// <param name="contextProperties">Context information to log with the exception.</param>
		/// <para/>Note: The <see cref="Exception.Data"/> dictionary
		/// may get modified for this exception.</param>
		/// <returns>The exception passed in as <paramref name="ex"/>.</returns>
		/// <remarks>
		/// See <see cref="Log&lt;T&gt;(T, Type)"/> for an example and more information.
		/// </remarks>
		public static T Log<T>(T ex, Type category, IDictionary<string, object> contextProperties)
			where T : Exception
		{
			// Make this a "quiet" precondition rather than throwing an exception
			// because the caller is already trying to throw an exception.
			if (category == null)
			{
				category = StackTraceUtility.GetSourceType(typeof(Exceptions));
			}

			Menees.Log.Error(category, null, ex, contextProperties);
			return ex;
		}

		/// <summary>
		/// Creates and logs a new InvalidOperationException with the specified message.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <returns>A new exception instance.</returns>
		public static InvalidOperationException NewInvalidOperationException(string message) => Log(new InvalidOperationException(message));

		/// <summary>
		/// Creates and logs a new InvalidOperationException with the specified message and context properties.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="contextProperties">Context information to log with the exception.</param>
		/// <returns>A new exception instance.</returns>
		public static InvalidOperationException NewInvalidOperationException(string message, IDictionary<string, object> contextProperties)
			=> Log(new InvalidOperationException(message), contextProperties);

		/// <summary>
		/// Enumerates all of the exceptions in an exception chain including handling multiple
		/// inner exceptions for <see cref="AggregateException"/>.
		/// </summary>
		/// <param name="ex">The root exception to start from.  <paramref name="action"/> will be called for this too.</param>
		/// <param name="action">The action to invoke for the root exception and each inner exception.  This is passed the
		/// exception and its 0-based depth (where 0 is the root exception).</param>
		public static void ForEach(Exception ex, Action<Exception, int> action)
		{
			Conditions.RequireReference(ex, () => ex);
			Conditions.RequireReference(action, () => action);

			ForEach(ex, 0, null, (exception, depth, outer) => action(exception, depth));
		}

		/// <summary>
		/// Enumerates all of the exceptions in an exception chain including handling multiple
		/// inner exceptions for <see cref="AggregateException"/>.
		/// </summary>
		/// <param name="ex">The root exception to start from.  <paramref name="action"/> will be called for this too.</param>
		/// <param name="action">The action to invoke for the root exception and each inner exception.  This is passed the
		/// exception, its 0-based depth (where 0 is the root exception), and the outer (i.e., parent) exception.</param>
		public static void ForEach(Exception ex, Action<Exception, int, Exception> action)
		{
			Conditions.RequireReference(ex, () => ex);
			Conditions.RequireReference(action, () => action);

			ForEach(ex, 0, null, action);
		}

		/// <summary>
		/// Builds a message from all of the exceptions in an exception tree including handling
		/// multiple inner exceptions for <see cref="AggregateException"/>s.
		/// </summary>
		/// <param name="ex">The root exception to start from.</param>
		/// <returns>A string containing the tab-indented messages from each exception where
		/// the indention level is based on the exception's depth in the tree.</returns>
		public static string GetMessage(Exception ex)
		{
			StringBuilder sb = new StringBuilder();
			Exceptions.ForEach(ex, (exception, depth, outer) => sb.Append('\t', depth).Append(exception.Message).AppendLine());
			string result = sb.ToString().Trim();
			return result;
		}

		#endregion

		#region Private Methods

		private static void ForEach(Exception ex, int depth, Exception parent, Action<Exception, int, Exception> action)
		{
			action(ex, depth, parent);

			depth++;
			if (ex is AggregateException aggregate)
			{
				foreach (Exception inner in aggregate.InnerExceptions)
				{
					ForEach(inner, depth, ex, action);
				}
			}
			else
			{
				Exception inner = ex.InnerException;
				if (inner != null)
				{
					ForEach(inner, depth, ex, action);
				}
			}
		}

		#endregion
	}
}
