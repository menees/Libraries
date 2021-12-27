namespace Menees.Diagnostics
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Runtime.Serialization.Json;
	using System.Threading.Tasks;

	#endregion

	/// <summary>
	/// Represents a single release of an application or library.
	/// </summary>
	public sealed class Release
	{
		#region Constructors

		private Release(string name, Version version, DateTime releasedUtc, Uri htmlUri)
		{
			this.Name = name;
			this.Version = version;
			this.ReleasedUtc = releasedUtc;
			this.HtmlUri = htmlUri;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the name of the release.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the version of the release.
		/// </summary>
		public Version Version { get; }

		/// <summary>
		/// Gets the UTC time of the release.
		/// </summary>
		public DateTime ReleasedUtc { get; }

		/// <summary>
		/// Gets the URI of the release's web page.
		/// </summary>
		public Uri HtmlUri { get; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Tries to get the latest release for the specified GitHub repository.
		/// </summary>
		/// <param name="repository">The name of a GitHub repository.</param>
		/// <param name="repositoryOwner">The owner of the GitHub repository.</param>
		/// <returns>The latest release info if found and parsed successfully; otherwise null.</returns>
		public static Release? FindGithubLatest(string repository, string repositoryOwner = "menees")
		{
			Conditions.RequireString(repository, nameof(repository));
			Conditions.RequireString(repositoryOwner, nameof(repositoryOwner));

			Release? result = null;

			using (HttpClient httpClient = new())
			{
				// The GitHub API requires a User-Agent header, and they request that it include the GitHub user name.
				// Unfortunately, .NET's ProductInfoHeaderValue doesn't support User-Agent comments, so we have
				// to add it without validation.
				// https://docs.github.com/en/rest/overview/resources-in-the-rest-api#user-agent-required
				// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent
				// https://github.com/dotnet/runtime/issues/28021
				string comment = $"{repositoryOwner} ({ApplicationInfo.ApplicationName}, {nameof(FindGithubLatest)} in {typeof(Release).Assembly.FullName})";
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", comment);

				try
				{
					Uri requestUri = new($"https://api.github.com/repos/{repositoryOwner}/{repository}/releases/latest");
					result = RequestLatestReleaseAsync(httpClient, requestUri).ConfigureAwait(false).GetAwaiter().GetResult();
				}
				catch (HttpRequestException ex)
				{
					Log.Error(typeof(Release), "Error from GitHub API request.", ex);
				}
			}

			return result;
		}

		/// <summary>
		/// Gets the <see cref="Name"/> of the release.
		/// </summary>
		public override string ToString() => this.Name;

		#endregion

		#region Private Methods

		private static async Task<Release?> RequestLatestReleaseAsync(HttpClient httpClient, Uri requestUri)
		{
			Release? result = null;

			using (HttpResponseMessage response = await httpClient.GetAsync(requestUri).ConfigureAwait(false))
			{
				if (response.StatusCode == HttpStatusCode.OK && response.Content.Headers.ContentType?.MediaType == "application/json")
				{
					using (Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
					{
						// This only gets the top-level properties (not nested dictionaries or arrays),
						// but it works in .NET Framework and .NET Core.
						var settings = new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
						var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, object>), settings);
						var typedProperties = (Dictionary<string, object>?)serializer.ReadObject(stream);
						var properties = typedProperties?.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString());

						const DateTimeStyles Styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
						if (properties != null
							&& properties.TryGetValue("tag_name", out string? tagName)
							&& properties.TryGetValue("name", out string? name)
							&& name != null
							&& TryParseVersion(tagName, name, out Version? version)
							&& properties.TryGetValue("published_at", out string? published)
							&& DateTime.TryParseExact(published, @"yyyy-MM-dd\THH:mm:ss\Z", CultureInfo.InvariantCulture, Styles, out DateTime releasedUtc)
							&& properties.TryGetValue("html_url", out string? htmlUriText)
							&& Uri.TryCreate(htmlUriText, UriKind.Absolute, out Uri? htmlUri))
						{
							result = new Release(name, version, releasedUtc, htmlUri);
						}
					}
				}
			}

			return result;
		}

		private static bool TryParseVersion(string? tagName, string? name, [MaybeNullWhen(false)] out Version version)
		{
			bool result = Version.TryParse(name, out version);

			if (!result && tagName != null)
			{
				const string TagVersionPrefix = "v";
				result = Version.TryParse(tagName, out version)
					|| (tagName.StartsWith(TagVersionPrefix, StringComparison.OrdinalIgnoreCase)
						&& Version.TryParse(tagName.Substring(TagVersionPrefix.Length), out version));
			}

			return result;
		}

		#endregion
	}
}
