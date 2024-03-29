namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;

	#endregion

	#region public ISettingsNode

	/// <summary>
	/// Provides a generic interface for a single node in an <see cref="ISettingsStore"/>.
	/// </summary>
	public interface ISettingsNode
	{
		#region Public Properties

		/// <summary>
		/// Gets the name of the current node.
		/// </summary>
		string NodeName { get; }

		/// <summary>
		/// Gets the number of settings in the current node.
		/// </summary>
		int SettingCount { get; }

		/// <summary>
		/// Gets the number of sub-nodes of the current node.
		/// </summary>
		int SubNodeCount { get; }

		/// <summary>
		/// Gets the parent settings node.
		/// </summary>
		ISettingsNode? ParentNode { get; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets a setting's value as a string.
		/// </summary>
		/// <param name="settingName">The name of a setting.</param>
		/// <param name="defaultValue">The default value to return if the setting isn't found.</param>
		/// <returns>The setting's current value or the default value.</returns>
		string GetValue(string settingName, string defaultValue);

		/// <summary>
		/// Gets a setting's value as a string.
		/// </summary>
		/// <param name="settingName">The name of a setting.</param>
		/// <param name="defaultValue">The default value to return if the setting isn't found.</param>
		/// <returns>The setting's current value or the default value.</returns>
		string? GetValueN(string settingName, string? defaultValue);

		/// <summary>
		/// Sets a setting's value as a string.
		/// </summary>
		/// <param name="settingName">The name of a new or existing setting.</param>
		/// <param name="value">The new value for the setting.</param>
		void SetValue(string settingName, string? value);

		/// <summary>
		/// Gets a setting's value as an Int32.
		/// </summary>
		/// <param name="settingName">The name of a setting.</param>
		/// <param name="defaultValue">The default value to return if the setting isn't found.</param>
		/// <returns>The setting's current value or the default value.</returns>
		int GetValue(string settingName, int defaultValue);

		/// <summary>
		/// Sets a setting's value as a Int32.
		/// </summary>
		/// <param name="settingName">The name of a new or existing setting.</param>
		/// <param name="value">The new value for the setting.</param>
		void SetValue(string settingName, int value);

		/// <summary>
		/// Gets a setting's value as a boolean.
		/// </summary>
		/// <param name="settingName">The name of a setting.</param>
		/// <param name="defaultValue">The default value to return if the setting isn't found.</param>
		/// <returns>The setting's current value or the default value.</returns>
		bool GetValue(string settingName, bool defaultValue);

		/// <summary>
		/// Sets a setting's value as a Boolean.
		/// </summary>
		/// <param name="settingName">The name of a new or existing setting.</param>
		/// <param name="value">The new value for the setting.</param>
		void SetValue(string settingName, bool value);

		/// <summary>
		/// Gets a setting's value as an enum.
		/// </summary>
		/// <param name="settingName">The name of a setting.</param>
		/// <param name="defaultValue">The default value to return if the setting isn't found.</param>
		/// <returns>The setting's current value or the default value.</returns>
		T GetValue<T>(string settingName, T defaultValue)
			where T : struct;

		/// <summary>
		/// Sets a setting's value as an enum.
		/// </summary>
		/// <param name="settingName">The name of a new or existing setting.</param>
		/// <param name="value">The new value for the setting.</param>
		void SetValue<T>(string settingName, T value)
			where T : struct;

		/// <summary>
		/// Gets the names of all the settings in the current node.
		/// </summary>
		/// <returns>A collection of setting names.</returns>
		IList<string> GetSettingNames();

		/// <summary>
		/// Deletes a setting from the current node.
		/// </summary>
		/// <param name="settingName">The name of a setting.</param>
		void DeleteSetting(string settingName);

		/// <summary>
		/// Gets the names of all the sub-nodes of the current node.
		/// </summary>
		/// <returns>A collection of node names.</returns>
		IList<string> GetSubNodeNames();

		/// <summary>
		/// Recursively deletes the sub-node with the specified name.
		/// </summary>
		/// <param name="nodeNameOrPath">The name or '\'-separated path of a sub-node.</param>
		void DeleteSubNode(string nodeNameOrPath);

		/// <summary>
		/// Gets an existing sub-node or creates a new sub-node with the specified name.
		/// </summary>
		/// <param name="nodeNameOrPath">The name or '\'-separated path of a sub-node.</param>
		/// <returns>An existing node if one is found or a new node if necessary.</returns>
		ISettingsNode GetSubNode(string nodeNameOrPath);

		/// <summary>
		/// Gets a sub-node if it already exists.
		/// </summary>
		/// <param name="nodeNameOrPath">The name or '\'-separated path of a sub-node.</param>
		/// <returns>An existing node if one is found, or null otherwise.</returns>
		ISettingsNode? TryGetSubNode(string nodeNameOrPath);

		#endregion
	}

	#endregion

	#region public ISettingsStore

	/// <summary>
	/// Provides a generic interface for working with a hierarchy of <see cref="ISettingsNode"/>s.
	/// </summary>
	public interface ISettingsStore : IDisposable
	{
		/// <summary>
		/// Gets the root node in the store.
		/// </summary>
		ISettingsNode RootNode { get; }

		/// <summary>
		/// Saves any changes that have been made to settings and nodes.
		/// </summary>
		void Save();
	}

	#endregion
}
