namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;

	#endregion

	/// <summary>
	/// Used to save and load a window's position and state.
	/// </summary>
	public sealed class WindowSaver
	{
		#region Constructors

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public WindowSaver(Window window)
		{
			Conditions.RequireReference(window, nameof(window));

			this.AutoLoad = true;
			this.AutoSave = true;
			this.SettingsNodeName = "Window Placement";
			this.Window = window;
			this.Window.SourceInitialized += this.Window_SourceInitialized;
			this.Window.Closing += this.Window_Closing;
		}

		#endregion

		#region Public Events

		/// <summary>
		/// Called when settings are being loaded.
		/// </summary>
		public event EventHandler<SettingsEventArgs>? LoadSettings;

		/// <summary>
		/// Called when settings are being saved.
		/// </summary>
		public event EventHandler<SettingsEventArgs>? SaveSettings;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets whether the settings should automatically load when the window loads.  This defaults to true.
		/// </summary>
		public bool AutoLoad { get; set; }

		/// <summary>
		/// Gets or sets whether the settings should automatically save when the window closes.  This defaults to true.
		/// </summary>
		public bool AutoSave { get; set; }

		/// <summary>
		/// Gets or sets the node where settings should be saved.  This can be empty to save to the root node.
		/// This defaults to "Window Placement".
		/// </summary>
		public string SettingsNodeName { get; set; }

		/// <summary>
		/// Gets the window to save the settings for.
		/// </summary>
		public Window Window { get; }

		/// <summary>
		/// Gets or sets a window state to use during <see cref="Load"/> instead of any prior state loaded from settings.
		/// This defaults to null, which means "don't use an override".
		/// </summary>
		public WindowState? LoadStateOverride { get; set; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Loads the heights for the <see cref="RowDefinition"/>s before and after a <see cref="GridSplitter"/>.
		/// </summary>
		/// <param name="baseNode">The base settings node to look for <see cref="settingsNodeName"/> under.</param>
		/// <param name="splitter">The grid splitter to load settings for.</param>
		/// <param name="settingsNodeName">The node name where the <paramref name="splitter"/>'s settings were saved.</param>
		public static void LoadSplits(ISettingsNode baseNode, GridSplitter splitter, string settingsNodeName = nameof(GridSplitter))
		{
			Conditions.RequireReference(baseNode, nameof(baseNode));
			Conditions.RequireReference(splitter, nameof(splitter));

			ISettingsNode? splitterNode = baseNode.TryGetSubNode(settingsNodeName);
			if (splitterNode != null)
			{
				RowDefinition[] splitterTargetRows = GetTargetRows(splitter);
				for (int i = 0; i < splitterTargetRows.Length; i++)
				{
					ISettingsNode? rowNode = splitterNode.TryGetSubNode($"Row{i}");
					if (rowNode != null)
					{
						double value = rowNode.GetValue(nameof(GridLength.Value), 1.0);
						GridUnitType unitType = rowNode.GetValue(nameof(GridLength.GridUnitType), GridUnitType.Star);
						RowDefinition row = splitterTargetRows[i];
						row.Height = new GridLength(value, unitType);
					}
				}
			}
		}

		/// <summary>
		/// Saves the heights for the <see cref="RowDefinition"/>s before and after a <see cref="GridSplitter"/>.
		/// </summary>
		/// <param name="baseNode">The base settings node to (re)create <paramref name="settingsNodeName"/> under.</param>
		/// <param name="splitter">The grid splitter to save settings for.</param>
		/// <param name="settingsNodeName">The node name where the <paramref name="splitter"/>'s settings should be saved.</param>
		public static void SaveSplits(ISettingsNode baseNode, GridSplitter splitter, string settingsNodeName = nameof(GridSplitter))
		{
			Conditions.RequireReference(baseNode, nameof(baseNode));
			Conditions.RequireReference(splitter, nameof(splitter));

			baseNode.DeleteSubNode(settingsNodeName);
			RowDefinition[] splitterTargetRows = GetTargetRows(splitter);
			if (splitterTargetRows.Length > 0)
			{
				ISettingsNode splitterNode = baseNode.GetSubNode(settingsNodeName);
				for (int i = 0; i < splitterTargetRows.Length; i++)
				{
					RowDefinition row = splitterTargetRows[i];
					GridLength rowHeight = row.Height;
					ISettingsNode rowNode = splitterNode.GetSubNode($"Row{i}");
					rowNode.SetValue(nameof(rowHeight.Value), rowHeight.Value);
					rowNode.SetValue(nameof(rowHeight.GridUnitType), rowHeight.GridUnitType);
				}
			}
		}

		/// <summary>
		/// Loads the window settings manually, which is useful if <see cref="AutoLoad"/> is false.
		/// </summary>
		/// <returns>True if the previous settings were re-loaded; false if no previous settings existed.</returns>
		public bool Load()
		{
			bool result = false;

			if (!WindowsUtility.IsInDesignMode(this.Window))
			{
				using (ISettingsStore store = ApplicationInfo.CreateUserSettingsStore())
				{
					ISettingsNode? settingsNode = this.GetSettingsNode(store, false);
					if (settingsNode != null)
					{
						NativeMethods.LoadWindowPlacement(this.Window, settingsNode, this.LoadStateOverride);
						result = true;
					}
					else if (this.LoadStateOverride != null)
					{
						this.Window.WindowState = this.LoadStateOverride.Value;
					}

					this.LoadSettings?.Invoke(this, new SettingsEventArgs(store.RootNode));
				}
			}

			return result;
		}

		/// <summary>
		/// Saves the window settings manually, which is useful if <see cref="AutoSave"/> is false.
		/// </summary>
		public void Save()
		{
			if (!WindowsUtility.IsInDesignMode(this.Window))
			{
				using (ISettingsStore store = ApplicationInfo.CreateUserSettingsStore())
				{
					ISettingsNode? settingsNode = this.GetSettingsNode(store, true);
					if (settingsNode != null)
					{
						NativeMethods.SaveWindowPlacement(this.Window, settingsNode);
					}

					this.SaveSettings?.Invoke(this, new SettingsEventArgs(store.RootNode));

					// Make sure the settings get saved out.
					store.Save();
				}
			}
		}

		#endregion

		#region Private Methods

		private static RowDefinition[] GetTargetRows(GridSplitter splitter)
		{
			RowDefinition[] result = Array.Empty<RowDefinition>();

			if (splitter.Parent is Grid grid)
			{
				int splitterRow = Grid.GetRow(splitter);
				result = new[]
				{
					grid.RowDefinitions[splitterRow - 1],
					grid.RowDefinitions[splitterRow + 1],
				};
			}

			return result;
		}

		private ISettingsNode? GetSettingsNode(ISettingsStore store, bool createIfNotFound)
		{
			ISettingsNode? result = store.RootNode;

			if (!string.IsNullOrEmpty(this.SettingsNodeName))
			{
				result = createIfNotFound ? result.GetSubNode(this.SettingsNodeName) : result.TryGetSubNode(this.SettingsNodeName);
			}

			return result;
		}

		#endregion

		#region Private Event Handlers

		private void Window_Closing(object? sender, CancelEventArgs e)
		{
			if (this.AutoSave && !e.Cancel)
			{
				this.Save();
			}
		}

		private void Window_SourceInitialized(object? sender, EventArgs e)
		{
			if (this.AutoLoad)
			{
				this.Load();
			}
		}

		#endregion
	}
}
