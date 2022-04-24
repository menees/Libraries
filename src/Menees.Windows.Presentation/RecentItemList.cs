namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;

	#endregion

	/// <summary>
	/// Manages a list of recent items and optionally their associated menu items.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class RecentItemList<T> : IReadOnlyList<T>, ICollection<T>, IDisposable
		where T : notnull
	{
		#region Public Constants

		/// <summary>
		/// The default value for <see cref="MaxItemCount"/>.
		/// </summary>
		public const byte DefaultMaxItemCount = 10;

		#endregion

		#region Private Data Members

		private const string DefaultSettingsNodeName = "Recent Items";
		private const string StringSettingName = "Item";

		private readonly List<T> list;
		private readonly List<ItemsControl> menus;
		private Action<T> itemClicked;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="mainMenuItem">A sub-menu item to add recent items to.
		/// This should typically be under the MainMenu's File menu, and the sub-item should be named "Recent".</param>
		/// <param name="itemClicked">The action to invoke when a recent menu item is clicked.</param>
		/// <param name="toolbarDropDownMenu">An optional context menu to add recent items to.
		/// This should typically be a drop-down menu under a toolbar "split button".
		/// </param>
		/// <param name="maxItemCount">The maximum number of items to store in the list.</param>
		public RecentItemList(
			MenuItem mainMenuItem,
			Action<T> itemClicked,
			ContextMenu? toolbarDropDownMenu = null,
			byte maxItemCount = DefaultMaxItemCount)
		{
			Conditions.RequireReference(mainMenuItem, nameof(mainMenuItem));
			Conditions.RequireReference(itemClicked, nameof(itemClicked));
			Conditions.RequireArgument(maxItemCount > 0, "The max item count must be positive.", nameof(maxItemCount));

			this.list = new(maxItemCount);
			this.itemClicked = itemClicked;
			this.MaxItemCount = maxItemCount;

			this.menus = new(2);
			this.menus.Add(mainMenuItem);
			if (toolbarDropDownMenu != null)
			{
				this.menus.Add(toolbarDropDownMenu);
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the number of items in the list.
		/// </summary>
		public int Count => this.list.Count;

		/// <summary>
		/// Gets the maximum number of items to keep in the list.
		/// </summary>
		public byte MaxItemCount { get; }

		/// <summary>
		/// Always returns false.
		/// </summary>
		bool ICollection<T>.IsReadOnly => ((ICollection<T>)this.list).IsReadOnly;

		/// <summary>
		/// Gets an item by numeric index.
		/// </summary>
		public T this[int index] => this.list[index];

		#endregion

		#region Public Methods

		/// <summary>
		/// A helper method to pass to <see cref="Load(ISettingsNode, Func{ISettingsNode, T}, string)"/>
		/// for the common case of loading a string item from settings.
		/// </summary>
		/// <param name="itemNode">The item node to load from.</param>
		/// <returns>The string value stored in the item node.</returns>
		public static string LoadString(ISettingsNode itemNode) => itemNode.GetValue(StringSettingName, string.Empty);

		/// <summary>
		/// A helper method to pass to <see cref="Save(ISettingsNode, Action{ISettingsNode, T}, string)"/>
		/// for the common case of saving a string item to settings.
		/// </summary>
		/// <param name="itemNode">The item node to save to.</param>
		/// <param name="item">The string value to store in the item node.</param>
		public static void SaveString(ISettingsNode itemNode, string item) => itemNode.SetValue(StringSettingName, item);

		/// <summary>
		/// Adds an item to the list at the top (i.e., index 0).
		/// </summary>
		/// <remarks>
		/// If <paramref name="item"/> is already in the list, then it is moved to the top (i.e., index 0).
		/// </remarks>
		public void Add(T item)
		{
			bool previouslyInList = this.Remove(item);
			this.list.Insert(0, item);

			if (!previouslyInList)
			{
				this.Crop();
			}

			this.UpdateMenus();
		}

		/// <summary>
		/// Removes all items from the list.
		/// </summary>
		public void Clear()
		{
			this.list.Clear();
			this.UpdateMenus();
		}

		/// <summary>
		/// Checks whether the item is in the list.
		/// </summary>
		public bool Contains(T item) => this.list.Contains(item);

		/// <summary>
		/// Copies the items to a target array.
		/// </summary>
		public void CopyTo(T[] array, int arrayIndex) => this.list.CopyTo(array, arrayIndex);

		/// <summary>
		/// Detaches from menu event handlers and release resources.
		/// </summary>
		public void Dispose()
		{
			this.Clear();
			this.menus.Clear();
			this.itemClicked = item => { };
		}

		/// <summary>
		/// Gets an enumerator over the list.
		/// </summary>
		public IEnumerator<T> GetEnumerator() => this.list.GetEnumerator();

		/// <summary>
		/// Loads the recent items from the <paramref name="settingsNodeName"/> node under <paramref name="baseNode"/>.
		/// </summary>
		/// <param name="baseNode">The base settings node to look for <see cref="settingsNodeName"/> under.</param>
		/// <param name="loadItem">The function used to load an item from an item's sub-node.</param>
		/// <param name="settingsNodeName">The node name where recent item settings were saved.</param>
		public void Load(ISettingsNode baseNode, Func<ISettingsNode, T> loadItem, string settingsNodeName = DefaultSettingsNodeName)
		{
			Conditions.RequireReference(baseNode, nameof(baseNode));
			Conditions.RequireReference(loadItem, nameof(loadItem));
			Conditions.RequireString(settingsNodeName, nameof(settingsNodeName));

			this.Clear();

			ISettingsNode? settingsNode = baseNode.TryGetSubNode(settingsNodeName);
			if (settingsNode != null)
			{
				List<T> loaded = new(this.MaxItemCount);
				for (int index = 0; index < this.MaxItemCount; index++)
				{
					ISettingsNode? itemNode = settingsNode.TryGetSubNode(index.ToString());
					if (itemNode == null)
					{
						break;
					}

					T item = loadItem(itemNode);
					loaded.Add(item);
				}

				// Use Distinct here in case someone manually edits a settings file to introduce a duplicate.
				// We don't need to Crop here since MaxItemCount is enforced in the loop condition.
				this.list.AddRange(loaded.Distinct());
				this.UpdateMenus();
			}
		}

		/// <summary>
		/// Tries to remove the item from the list.
		/// </summary>
		public bool Remove(T item)
		{
			bool result = this.list.Remove(item);
			if (result)
			{
				this.UpdateMenus();
			}

			return result;
		}

		/// <summary>
		/// Saves the recent items to the <paramref name="settingsNodeName"/> node under <paramref name="baseNode"/>.
		/// </summary>
		/// <param name="baseNode">The base settings node to (re)create <paramref name="settingsNodeName"/> under.</param>
		/// <param name="saveItem">The action used to save an item into its own sub-node.</param>
		/// <param name="settingsNodeName">The node name where recent item settings should be saved.</param>
		public void Save(ISettingsNode baseNode, Action<ISettingsNode, T> saveItem, string settingsNodeName = DefaultSettingsNodeName)
		{
			Conditions.RequireReference(baseNode, nameof(baseNode));
			Conditions.RequireReference(saveItem, nameof(saveItem));
			Conditions.RequireString(settingsNodeName, nameof(settingsNodeName));

			// Clear out any old settings.
			baseNode.DeleteSubNode(settingsNodeName);

			ISettingsNode settingsNode = baseNode.GetSubNode(settingsNodeName);
			int index = 0;
			foreach (T item in this.list)
			{
				ISettingsNode itemNode = settingsNode.GetSubNode(index.ToString());
				saveItem(itemNode, item);
				index++;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

		#endregion

		#region Private Methods

		private void UpdateMenus()
		{
			foreach (ItemsControl menu in this.menus)
			{
				// Remove old items so any references are released.
				foreach (RecentMenuItem menuItem in menu.Items.OfType<RecentMenuItem>())
				{
					menuItem.Click -= this.MenuItemClick;
				}

				menu.Items.Clear();

				if (this.Count == 0)
				{
					menu.Items.Add(new NoneMenuItem());
				}
				else
				{
					foreach (T item in this.list)
					{
						RecentMenuItem recent = new(item);
						recent.Click += this.MenuItemClick;
						menu.Items.Add(recent);
					}
				}
			}
		}

		private void MenuItemClick(object sender, RoutedEventArgs e)
		{
			if (sender is RecentMenuItem menuItem)
			{
				this.itemClicked(menuItem.Item);
			}
		}

		private void Crop()
		{
			while (this.list.Count > this.MaxItemCount)
			{
				this.list.RemoveAt(this.list.Count - 1);
			}
		}

		#endregion

		#region Private Types

		private abstract class CustomMenuItem : MenuItem
		{
			#region Constructors

			protected CustomMenuItem()
			{
				// TODO: Try to remove these binding errors in the debugger. [Bill, 4/24/2022]
				// These are needed to prevent warnings logged in the debugger like:
				// System.Windows.Data Error: 4 : Cannot find source for binding with reference 'RelativeSource FindAncestor,
				//     AncestorType='System.Windows.Controls.ItemsControl', AncestorLevel='1''. BindingExpression:Path=HorizontalContentAlignment;
				//     DataItem=null; target element is 'NoneMenuItem' (Name=''); target property is 'HorizontalContentAlignment' (type 'HorizontalAlignment')
				this.HorizontalContentAlignment = HorizontalAlignment.Left;
				this.VerticalContentAlignment = VerticalAlignment.Center;
			}

			#endregion
		}

		private sealed class NoneMenuItem : CustomMenuItem
		{
			#region Constructors

			public NoneMenuItem()
			{
				this.Header = "<None>";
				this.IsEnabled = false;
			}

			#endregion
		}

		private sealed class RecentMenuItem : CustomMenuItem
		{
			#region Constructors

			public RecentMenuItem(T item)
			{
				this.Header = item.ToString();
				this.Item = item;
			}

			#endregion

			#region Public Properties

			public T Item { get; }

			#endregion
		}

		#endregion
	}
}
