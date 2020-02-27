namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;

	#endregion

	/// <summary>
	/// Extends the standard WPF Calendar control to support variable control
	/// sizes, fonts, and system colors.
	/// </summary>
	public sealed class ExtendedCalendar : Calendar
	{
		#region Public Fields

		/// <summary>
		/// The dependency property backing field for the <see cref="HeaderBrush"/> property.
		/// </summary>
		public static readonly DependencyProperty HeaderBrushProperty =
			DependencyProperty.Register(
			nameof(HeaderBrush),
			typeof(Brush),
			typeof(ExtendedCalendar),
			new PropertyMetadata(SystemColors.GradientInactiveCaptionBrush));

		#endregion

		#region Constructors

		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "WPF requires static constructors.")]
		static ExtendedCalendar()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ExtendedCalendar), new FrameworkPropertyMetadata(typeof(ExtendedCalendar)));
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the brush to use for the header background.
		/// </summary>
		public Brush HeaderBrush
		{
			get { return (Brush)this.GetValue(HeaderBrushProperty); }
			set { this.SetValue(HeaderBrushProperty, value); }
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Works around a bug in the WPF Calendar control where it fails to correctly release the mouse capture.
		/// </summary>
		protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
		{
			// The stupid Calendar control can capture the mouse and keep it too long, which makes
			// users have to click twice to get focus out of the control and on to another control.
			// http://stackoverflow.com/questions/2425951/wpf-toolkit-calendar-takes-two-clicks-to-get-focus
			base.OnPreviewMouseUp(e);

			if (Mouse.Captured is CalendarItem)
			{
				Mouse.Capture(null);
			}
		}

		#endregion
	}
}
