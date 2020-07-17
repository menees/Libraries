namespace Menees.Diagnostics
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Mime;
	using System.Reflection;
	using System.Runtime.Serialization.Json;
	using System.Text;
	using Menees.Shell;

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
		public static Release FindGithubLatest(string repository, string repositoryOwner = "menees")
		{
			Conditions.RequireString(repository, nameof(repository));
			Conditions.RequireString(repositoryOwner, nameof(repositoryOwner));

			Release result = null;

			// HttpWebRequest works in .NET Framework and .NET Core.
			HttpWebRequest request = WebRequest.CreateHttp(new Uri($"https://api.github.com/repos/{repositoryOwner}/{repository}/releases/latest"));

			// The GitHub API requires a User-Agent header, and they request that it include the GitHub user name.
			// https://developer.github.com/v3/#user-agent-required
			request.UserAgent = repositoryOwner + ", " + typeof(Release).Assembly.FullName;

			try
			{
				using (var response = (HttpWebResponse)request.GetResponse())
				{
					if (response.StatusCode == HttpStatusCode.OK && new ContentType(response.ContentType).MediaType == "application/json")
					{
						using (Stream stream = response.GetResponseStream())
						{
							// This only gets the top-level properties (not nested dictionaries or arrays),
							// but it works in .NET Framework and .NET Core.
							var settings = new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
							var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, object>), settings);
							var typedProperties = (Dictionary<string, object>)serializer.ReadObject(stream);
							var properties = typedProperties.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString());

							const DateTimeStyles Styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
							if (properties.TryGetValue("tag_name", out string tagName)
								&& properties.TryGetValue("name", out string name)
								&& TryParseVersion(tagName, name, out Version version)
								&& properties.TryGetValue("published_at", out string published)
								&& DateTime.TryParseExact(published, @"yyyy-MM-dd\THH:mm:ss\Z", CultureInfo.InvariantCulture, Styles, out DateTime releasedUtc)
								&& properties.TryGetValue("html_url", out string htmlUriText)
								&& Uri.TryCreate(htmlUriText, UriKind.Absolute, out Uri htmlUri))
							{
								result = new Release(name, version, releasedUtc, htmlUri);
							}
						}
					}
				}
			}
			catch (WebException ex)
			{
				Log.Error(typeof(Release), "Error from GitHub API request.", ex);
			}

			return result;
		}

		/// <summary>
		/// Checks if the requested repository's <see cref="Version"/> is newer than the assembly's version
		/// and shows the updated release web page and message if necessary.
		/// </summary>
		/// <param name="assembly">The assembly to compare to. This is required.</param>
		/// <param name="ownerHandle">An optional owner window handle if a newer release's web page should be opened.</param>
		/// <param name="showMessage">An optional action to show a MessageBox if app messages should be displayed.</param>
		/// <param name="repository">The name of a GitHub repository. If null, then <see cref="ApplicationInfo.ApplicationName"/> is used.</param>
		/// <param name="repositoryOwner">The owner of the GitHub repository.</param>
		/// <returns>True if an update is available. False if the assembly is up-to-date. Null if no release info was found.</returns>
		public static bool? CheckForUpdate(
			Assembly assembly,
			IntPtr? ownerHandle,
			Action<string> showMessage,
			string repository = null,
			string repositoryOwner = "menees")
		{
			bool? result = null;

			Release release = FindGithubLatest(repository ?? ApplicationInfo.ApplicationName, repositoryOwner);
			if (release == null)
			{
				showMessage?.Invoke("Unable to determine the latest release information.");
			}
			else
			{
				result = release.CheckForUpdate(assembly, ownerHandle, showMessage);
			}

			return result;
		}

		/// <summary>
		/// Checks if the current <see cref="Version"/> is newer than the assembly's version
		/// and shows the updated release web page and message if necessary.
		/// </summary>
		/// <param name="assembly">The assembly to compare to. This is required.</param>
		/// <param name="ownerHandle">An optional owner window handle if a newer release's web page should be opened.</param>
		/// <param name="showMessage">An optional action to show a MessageBox if app messages should be displayed.</param>
		/// <returns>True if an update is available. False otherwise (i.e., <paramref name="assembly"/> is up-to-date).</returns>
		public bool CheckForUpdate(Assembly assembly, IntPtr? ownerHandle, Action<string> showMessage)
			=> this.CheckForUpdate(ReflectionUtility.GetVersion(assembly), ownerHandle, showMessage);

		/// <summary>
		/// Checks if the current <see cref="Version"/> is newer than the specified <paramref name="fromVersion"/>
		/// and shows the updated release web page and message if necessary.
		/// </summary>
		/// <param name="fromVersion">The version to compare to. This is required.</param>
		/// <param name="ownerHandle">An optional owner window handle if a newer release's web page should be opened.</param>
		/// <param name="showMessage">An optional action to show a MessageBox if app messages should be displayed.</param>
		/// <returns>True if an update is available. False otherwise (i.e., <paramref name="fromVersion"/> is up-to-date).</returns>
		public bool CheckForUpdate(Version fromVersion, IntPtr? ownerHandle, Action<string> showMessage)
		{
			Conditions.RequireReference(fromVersion, nameof(fromVersion));
			bool result = fromVersion < this.Version;

			if (!result)
			{
				showMessage?.Invoke("You're up-to-date!");
			}
			else
			{
				if (ownerHandle != null)
				{
					ShellUtility.ShellExecute(ownerHandle, this.HtmlUri.ToString());
				}

				if (showMessage != null)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append("You're on version ").Append(fromVersion).Append(", but ");
					sb.Append(this.Version).Append(" has been available since ");
					sb.Append(this.ReleasedUtc.ToLocalTime()).AppendLine(" from ").Append(this.HtmlUri).Append('.');
					showMessage(sb.ToString());
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

		private static bool TryParseVersion(string tagName, string name, out Version version)
		{
			bool result = Version.TryParse(name, out version);

			if (!result)
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
