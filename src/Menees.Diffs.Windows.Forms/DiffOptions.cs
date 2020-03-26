namespace Menees.Diffs.Windows.Forms
{
	#region Using Directives

	using System;
	using System.Diagnostics;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using Menees.Windows.Forms;

	#endregion

	public static class DiffOptions
	{
		#region Public Fields

		// I originally used PaleGreen, but it was too close to PaleTurquoise when I added intra-line diffs.
		public static readonly Color DefaultChangedColor = Color.FromArgb(200, 255, 200);
		public static readonly Color DefaultDeletedColor = Color.Pink;
		public static readonly Color DefaultInsertedColor = Color.PaleTurquoise;

		#endregion

		#region Private Data Members

		private const int DefaultSpacesPerTab = 4;

		private static bool changed;
		private static bool hatchDeadSpace;
		private static Color changedColor = DefaultChangedColor;
		private static Color deletedColor = DefaultDeletedColor;
		private static Color insertedColor = DefaultInsertedColor;
		private static int spacesPerTab = DefaultSpacesPerTab;
		private static int updateLevel;

		#endregion

		#region Public Events

		public static event EventHandler OptionsChanged;

		#endregion

		#region Public Properties

		public static Color ChangedColor
		{
			get
			{
				return changedColor;
			}

			set
			{
				if (changedColor != value)
				{
					BeginUpdate();
					changedColor = value;
					changed = true;
					EndUpdate();
				}
			}
		}

		public static Color DeletedColor
		{
			get
			{
				return deletedColor;
			}

			set
			{
				if (deletedColor != value)
				{
					BeginUpdate();
					deletedColor = value;
					changed = true;
					EndUpdate();
				}
			}
		}

		public static bool HatchDeadSpace
		{
			get
			{
				return hatchDeadSpace;
			}

			set
			{
				if (hatchDeadSpace != value)
				{
					BeginUpdate();
					hatchDeadSpace = value;
					changed = true;
					EndUpdate();
				}
			}
		}

		public static Color InsertedColor
		{
			get
			{
				return insertedColor;
			}

			set
			{
				if (insertedColor != value)
				{
					BeginUpdate();
					insertedColor = value;
					changed = true;
					EndUpdate();
				}
			}
		}

		public static int SpacesPerTab
		{
			get
			{
				return spacesPerTab;
			}

			set
			{
				if (spacesPerTab != value)
				{
					BeginUpdate();
					spacesPerTab = value;
					changed = true;
					EndUpdate();
				}
			}
		}

		#endregion

		#region Public Methods

		public static void BeginUpdate()
		{
			updateLevel++;
		}

		public static void EndUpdate()
		{
			updateLevel--;

			if (updateLevel == 0 && changed)
			{
				changed = false;
				OptionsChanged?.Invoke(null, EventArgs.Empty);
			}
		}

		public static Color GetColorForEditType(EditType editType)
		{
			Color result;

			switch (editType)
			{
				case EditType.Change:
					result = ChangedColor;
					break;

				case EditType.Insert:
					result = InsertedColor;
					break;

				case EditType.Delete:
					result = DeletedColor;
					break;

				default:
					Debug.Assert(false, "An invalid EditType was passed in.");
					result = Color.Transparent;
					break;
			}

			return result;
		}

		public static void Load(ISettingsNode node)
		{
			BeginUpdate();
			try
			{
				InsertedColor = Color.FromArgb(node.GetValue(nameof(InsertedColor), DefaultInsertedColor.ToArgb()));
				DeletedColor = Color.FromArgb(node.GetValue(nameof(DeletedColor), DefaultDeletedColor.ToArgb()));
				ChangedColor = Color.FromArgb(node.GetValue(nameof(ChangedColor), DefaultChangedColor.ToArgb()));
				SpacesPerTab = Math.Max(1, node.GetValue(nameof(SpacesPerTab), DefaultSpacesPerTab));
				HatchDeadSpace = node.GetValue(nameof(HatchDeadSpace), hatchDeadSpace);
			}
			finally
			{
				EndUpdate();
			}
		}

		public static void Save(ISettingsNode node)
		{
			node.SetValue(nameof(InsertedColor), InsertedColor.ToArgb());
			node.SetValue(nameof(DeletedColor), DeletedColor.ToArgb());
			node.SetValue(nameof(ChangedColor), ChangedColor.ToArgb());
			node.SetValue(nameof(SpacesPerTab), SpacesPerTab);
			node.SetValue(nameof(HatchDeadSpace), HatchDeadSpace);
		}

		public static Brush TryCreateDeadSpaceBrush(Color backColor)
		{
			Brush result = null;
			if (HatchDeadSpace)
			{
				result = new HatchBrush(HatchStyle.Percent25, SystemColors.ControlDark, backColor);
			}

			return result;
		}

		#endregion
	}
}
