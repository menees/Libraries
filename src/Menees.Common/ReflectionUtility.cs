namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using System.Text;

	#endregion

	/// <summary>
	/// Methods for getting metadata about assemblies, types, members, etc.
	/// </summary>
	public static class ReflectionUtility
	{
		#region Public Methods

		/// <summary>
		/// Gets the assembly's copyright information.
		/// </summary>
		/// <param name="assembly">The assembly to get the copyright from.</param>
		/// <returns>User-friendly copyright information.</returns>
		public static string? GetCopyright(Assembly assembly)
		{
			Conditions.RequireReference(assembly, nameof(assembly));

			string? result = null;

			var copyrights = (AssemblyCopyrightAttribute[])assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
			if (copyrights.Length > 0)
			{
				result = copyrights[0].Copyright;
			}

			return result;
		}

		/// <summary>
		/// Gets the assembly's version.
		/// </summary>
		/// <param name="assembly">The assembly to get the version from</param>
		/// <returns>The assembly version</returns>
		public static Version? GetVersion(Assembly assembly)
		{
			Conditions.RequireReference(assembly, nameof(assembly));
			Version? result = assembly.GetName(false).Version;
			return result;
		}

		/// <summary>
		/// Gets the UTC build timestamp from the assembly.
		/// </summary>
		/// <param name="assembly">The assembly to get the BuildTime metadata from.</param>
		/// <returns>The assembly's build time as a UTC datetime if an <see cref="AssemblyMetadataAttribute"/> is found
		/// with Key="BuildTime" and Value equal to a parsable UTC datetime. Returns null otherwise.</returns>
		public static DateTime? GetBuildTime(Assembly assembly)
		{
			Conditions.RequireReference(assembly, nameof(assembly));

			const DateTimeStyles UseUtc = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;
			DateTime? result = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
				.Where(metadata => string.Equals(metadata.Key, "BuildTime"))
				.Select(metadata => DateTime.TryParse(metadata.Value, null, UseUtc, out DateTime value) ? value : (DateTime?)null)
				.FirstOrDefault(value => value != null);

			if (result != null)
			{
				result = result.Value.Kind switch
				{
					DateTimeKind.Unspecified => DateTime.SpecifyKind(result.Value, DateTimeKind.Utc),
					DateTimeKind.Local => result.Value.ToUniversalTime(),
					_ => result.Value,
				};
			}

			return result;
		}

		/// <summary>
		/// Gets the ProductUrl from the assembly metadata.
		/// </summary>
		/// <param name="assembly">The assembly to get the ProductUrl metadata from.</param>
		/// <returns>The assembly's build time as a Uri if an <see cref="AssemblyMetadataAttribute"/> is found
		/// with Key="ProductUrl" and Value equal to a parsable absolute Uri. Returns null otherwise.</returns>
		public static Uri? GetProductUrl(Assembly assembly)
		{
			Conditions.RequireReference(assembly, nameof(assembly));

			Uri? result = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
				.Where(metadata => string.Equals(metadata.Key, "ProductUrl"))
				.Select(metadata => Uri.TryCreate(metadata.Value, UriKind.Absolute, out Uri? value) ? value : null)
				.FirstOrDefault(value => value != null);

			return result;
		}

		/// <summary>
		/// Gets whether the assembly was built with a debug configuration.
		/// </summary>
		/// <param name="assembly">The assembly to check.</param>
		/// <returns>True if the <see cref="AssemblyConfigurationAttribute"/> is present
		/// and the configuration string contains "Debug". False otherwise.</returns>
		public static bool IsDebugBuild(Assembly assembly)
		{
			Conditions.RequireReference(assembly, nameof(assembly));

			bool result = false;

			if (assembly != null)
			{
				var configuration = (AssemblyConfigurationAttribute?)assembly.GetCustomAttribute(typeof(AssemblyConfigurationAttribute));
				result = configuration?.Configuration?.Contains("Debug") ?? false;
			}

			return result;
		}

		#endregion
	}
}
