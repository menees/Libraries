namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using System.Security;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;

	#endregion

	internal sealed class FileSettingsStore : ISettingsStore
	{
		#region Private Data Members

		private readonly XElement root;

		#endregion

		#region Constructors

		public FileSettingsStore()
		{
			foreach (string fileName in GetPotentialFileNames())
			{
				// Make sure the file exists and isn't zero length.  If a process was killed while saving data,
				// then the settings store file may be empty.  If so, we'll pretend it isn't there.
				FileInfo fileInfo = new(fileName);
				if (fileInfo.Exists && fileInfo.Length > 0)
				{
					try
					{
						// Only read settings from a file that we have *write* access to so if we're running from a remote read-only file share,
						// then it will load a local saved file (e.g., from AppData) instead of loading one from the remote read-only share.
						using (FileStream stream = new(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
						{
							this.root = XElement.Load(stream);
						}

						break;
					}
					catch (XmlException ex)
					{
						Log.Warning(typeof(FileSettingsStore), $"Unable to load the user settings from {fileName}.", ex);
					}
					catch (Exception ex) when (Exceptions.IsAccessException(ex))
					{
						// Ignore the file if we don't have both read and write access to it. If we're running off a read-only share,
						// we know Save() can't write back to that location, so we shouldn't read in from that location on startup.
						Log.Debug(typeof(FileSettingsStore), $"Unable to get read/write access to {fileName}.", ex);
					}
				}
			}

			if (this.root == null)
			{
				this.root = FileSettingsNode.CreateNodeElement(ApplicationInfo.ApplicationName);
			}

			this.RootNode = new FileSettingsNode(this.root, null);
		}

		#endregion

		#region ISettingsStore Members

		public ISettingsNode RootNode
		{
			get;
		}

		public void Save()
		{
			bool saved = false;
			Dictionary<string, object> errorLogProperties = new();

			foreach (string fileName in GetPotentialFileNames())
			{
				try
				{
					// Make sure the directory we're trying to save to actually exists.
					string? directoryName = Path.GetDirectoryName(fileName);
					if (!string.IsNullOrEmpty(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}

					if (File.Exists(fileName))
					{
						// Make sure the file isn't hidden, so XElement.Save can write to it.
						File.SetAttributes(fileName, File.GetAttributes(fileName) & ~FileAttributes.Hidden);
					}

					// Attempt to save the file.
					this.root.Save(fileName);
					saved = true;

					// Hide the file from normal listings since we'll be creating files for each user.
					File.SetAttributes(fileName, File.GetAttributes(fileName) | FileAttributes.Hidden);
					break;
				}
				catch (Exception ex) when (Exceptions.IsAccessException(ex))
				{
					errorLogProperties.Add(fileName, ex);
				}
			}

			if (!saved)
			{
				throw Exceptions.NewInvalidOperationException("Unable to save the user settings to a file store.", errorLogProperties);
			}
		}

		public void Dispose()
		{
			// There's nothing to do here since XElement isn't disposable.
		}

		#endregion

		#region Private Methods

		private static IEnumerable<string> GetPotentialFileNames()
		{
			string fileName = ApplicationInfo.ApplicationName + "-" + Environment.UserDomainName + "-" + Environment.UserName + ".stgx";

			List<string> result = new();

			// First, try the application's base directory.  That gives the best isolation for side-by-side app usage,
			// but a non-admin user may not have permissions to write to this directory.  Side-by-side isolation is
			// important if old and new versions of the software are run on the same machine.  (New versions will
			// typically be able to read in old settings, but they'll only write out settings in the new format.  After
			// that "upgrade", the old version wouldn't be able to read the settings in the new format.)
			string path = Path.Combine(ApplicationInfo.BaseDirectory, fileName);
			result.Add(path);

			// Next, try a Menees folder under the user's AppData\Local folder.  The user should be able to write
			// to their local AppData folder if it exists (e.g., if the user is not in a roaming profile).
			string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (!string.IsNullOrEmpty(localAppDataPath))
			{
				path = Path.Combine(localAppDataPath, nameof(Menees), fileName);
				result.Add(path);
			}

			// If all else fails, try to use the current temp directory (which may or may not be user-specific);
			path = Path.Combine(Path.GetTempPath(), nameof(Menees), fileName);
			result.Add(path);

			return result;
		}

		#endregion

		#region Private Types

		private sealed class FileSettingsNode : ISettingsNode
		{
			#region Private Data Members

			private readonly XElement element;
			private readonly FileSettingsNode? parent;

			#endregion

			#region Constructors

			public FileSettingsNode(XElement element, FileSettingsNode? parent)
			{
				this.element = element;
				this.parent = parent;
			}

			#endregion

			#region ISettingsNode Members

			public string NodeName
			{
				get
				{
					string result = GetNodeName(this.element);
					return result;
				}
			}

			public int SettingCount
			{
				get
				{
					int result = this.GetSettingElements().Count();
					return result;
				}
			}

			public int SubNodeCount
			{
				get
				{
					int result = this.GetSubNodeElements().Count();
					return result;
				}
			}

			public ISettingsNode? ParentNode => this.parent;

			public string GetValue(string settingName, string defaultValue)
				=> this.GetValueN(settingName, defaultValue) ?? defaultValue;

			public string? GetValueN(string settingName, string? defaultValue)
			{
				string? result = defaultValue;

				XElement? settingElement = this.GetSettingElement(settingName, false);
				if (settingElement != null)
				{
					result = settingElement.GetAttributeValueN("Value", defaultValue);
				}

				return result;
			}

			public void SetValue(string settingName, string? value)
			{
				XElement settingElement = this.GetSettingElement(settingName, true)!;
				settingElement.SetAttributeValue("Value", value);
			}

			public int GetValue(string settingName, int defaultValue)
			{
				int result = defaultValue;

				XElement? settingElement = this.GetSettingElement(settingName, false);
				if (settingElement != null)
				{
					result = settingElement.GetAttributeValue("Value", defaultValue);
				}

				return result;
			}

			public void SetValue(string settingName, int value)
			{
				this.SetValue(settingName, Convert.ToString(value));
			}

			public bool GetValue(string settingName, bool defaultValue)
			{
				bool result = defaultValue;

				XElement? settingElement = this.GetSettingElement(settingName, false);
				if (settingElement != null)
				{
					result = settingElement.GetAttributeValue("Value", defaultValue);
				}

				return result;
			}

			public void SetValue(string settingName, bool value)
			{
				this.SetValue(settingName, Convert.ToString(value));
			}

			public T GetValue<T>(string settingName, T defaultValue)
				where T : struct
			{
				T result = defaultValue;

				XElement? settingElement = this.GetSettingElement(settingName, false);
				if (settingElement != null)
				{
					result = settingElement.GetAttributeValue("Value", defaultValue);
				}

				return result;
			}

			public void SetValue<T>(string settingName, T value)
				where T : struct
			{
				this.SetValue(settingName, Convert.ToString(value));
			}

			public IList<string> GetSettingNames()
			{
				var result = this.GetSettingElements().Select(e => GetSettingName(e)).ToList();
				return result;
			}

			public void DeleteSetting(string settingName)
			{
				XElement? settingElement = this.GetSettingElement(settingName, false);
				if (settingElement != null)
				{
					settingElement.Remove();
				}
			}

			public IList<string> GetSubNodeNames()
			{
				var result = this.GetSubNodeElements().Select(e => GetNodeName(e)).ToList();
				return result;
			}

			public void DeleteSubNode(string nodeNameOrPath)
			{
				XElement? subElement = this.GetSubNodeElement(nodeNameOrPath, false);
				if (subElement != null)
				{
					subElement.Remove();
				}
			}

			public ISettingsNode GetSubNode(string nodeNameOrPath)
			{
				XElement subElement = this.GetSubNodeElement(nodeNameOrPath, true)!;
				FileSettingsNode result = new(subElement, this);
				return result;
			}

			public ISettingsNode? TryGetSubNode(string nodeNameOrPath)
			{
				FileSettingsNode? result = null;

				XElement? subElement = this.GetSubNodeElement(nodeNameOrPath, false);
				if (subElement != null)
				{
					result = new FileSettingsNode(subElement, this);
				}

				return result;
			}

			#endregion

			#region Internal Methods

			internal static XElement CreateNodeElement(string nodeName)
			{
				Conditions.RequireString(nodeName, nameof(nodeName));
				XElement result = new("Settings", new XAttribute("Name", nodeName));
				return result;
			}

			#endregion

			#region Private Methods

			private static string GetNodeName(XElement nodeElement)
			{
				string result = nodeElement.GetAttributeValue("Name");
				return result;
			}

			private static string GetSettingName(XElement settingElement)
			{
				string result = settingElement.GetAttributeValue("Name");
				return result;
			}

			private static IEnumerable<XElement> GetSubNodeElements(XElement element)
			{
				var result = element.Elements("Settings");
				return result;
			}

			private IEnumerable<XElement> GetSubNodeElements()
			{
				var result = GetSubNodeElements(this.element);
				return result;
			}

			private IEnumerable<XElement> GetSettingElements()
			{
				var result = this.element.Elements("Setting");
				return result;
			}

			private XElement? GetSubNodeElement(string nodeNameOrPath, bool createIfNotFound)
			{
				Conditions.RequireString(nodeNameOrPath, nameof(nodeNameOrPath));

				XElement? result = null;
				XElement currentElement = this.element;

				// Use backslash as the path separator because that's what RegistryKey.OpenSubKey uses,
				// and callers need to be able to switch between the ISettingsStore types without breaking.
				// FWIW, RegistryKey ignores the forward slash character, so we can't split on it.
				string[] nodeNames = nodeNameOrPath.Split('\\');
				foreach (string nodeName in nodeNames)
				{
					result = GetSubNodeElements(currentElement).SingleOrDefault(e => GetNodeName(e) == nodeName);
					if (result == null)
					{
						if (!createIfNotFound)
						{
							break;
						}

						result = CreateNodeElement(nodeName);
						currentElement.Add(result);
					}

					currentElement = result;
				}

				return result;
			}

			private XElement? GetSettingElement(string settingName, bool createIfNotFound)
			{
				XElement? result = this.GetSettingElements().SingleOrDefault(e => GetSettingName(e) == settingName);
				if (result == null && createIfNotFound)
				{
					result = new XElement("Setting", new XAttribute("Name", settingName));
					this.element.Add(result);
				}

				return result;
			}

			#endregion
		}

		#endregion
	}
}
