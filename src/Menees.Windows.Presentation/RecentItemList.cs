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
		private readonly HashSet<ItemsControl> upToDateMenus;
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

			this.menus = new();
			this.menus.Add(mainMenuItem);
			mainMenuItem.SubmenuOpened += this.MenuOpening;
			if (toolbarDropDownMenu != null)
			{
				this.menus.Add(toolbarDropDownMenu);
				toolbarDropDownMenu.ContextMenuOpening += this.MenuOpening;
			}

			this.upToDateMenus = new(this.menus.Count);
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
			bool removed = this.Remove(item);
			this.list.Insert(0, item);

			if (!removed)
			{
				while (this.list.Count > this.MaxItemCount)
				{
					this.list.RemoveAt(this.list.Count - 1);
				}
			}

			this.SetOutOfDate();
		}

		/// <summary>
		/// Removes all items from the list.
		/// </summary>
		public void Clear()
		{
			this.list.Clear();
			this.SetOutOfDate();
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
			foreach (ItemsControl menu in this.menus)
			{
				switch (menu)
				{
					case MenuItem menuItem:
						menuItem.SubmenuOpened -= this.MenuOpening;
						break;

					case ContextMenu contextMenu:
						contextMenu.ContextMenuOpening -= this.MenuOpening;
						break;
				}
			}

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
				// Use a byte's range since that's the type for MaxItemCount.
				for (int index = byte.MinValue; index <= byte.MaxValue; index++)
				{
					ISettingsNode? itemNode = settingsNode.TryGetSubNode(index.ToString());
					if (itemNode == null)
					{
						break;
					}

					T item = loadItem(itemNode);
					this.list.Add(item);
				}
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
				this.SetOutOfDate();
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

		private void SetOutOfDate()
		{
			this.upToDateMenus.Clear();

			// Remove old items so any references are released.
			foreach (ItemsControl menu in this.menus)
			{
				this.RemoveOldMenuItems(menu);
			}
		}

		private void RemoveOldMenuItems(ItemsControl menu)
		{
			foreach (RecentMenuItem menuItem in menu.Items.OfType<RecentMenuItem>())
			{
				menuItem.Click -= this.MenuItemClick;
			}

			menu.Items.Clear();
		}

		private void MenuOpening(object sender, RoutedEventArgs e)
		{
			if (sender is ItemsControl menu && !this.upToDateMenus.Contains(menu))
			{
				this.RemoveOldMenuItems(menu);

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

				this.upToDateMenus.Add(menu);
			}
		}

		private void MenuItemClick(object sender, RoutedEventArgs e)
		{
			if (sender is RecentMenuItem menuItem)
			{
				this.itemClicked(menuItem.Item);
			}
		}

		#endregion

		#region Private Types

		private sealed class NoneMenuItem : MenuItem
		{
			#region Constructors

			public NoneMenuItem()
			{
				this.Header = "<None>";
				this.IsEnabled = false;
			}

			#endregion
		}

		private sealed class RecentMenuItem : MenuItem
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
