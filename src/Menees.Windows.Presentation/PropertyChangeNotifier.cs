namespace Menees.Windows.Presentation
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Runtime.CompilerServices;
	using System.Text;

	#endregion

	/// <summary>
	/// Provides a simple implementation of the <see cref="INotifyPropertyChanged"/> interface for data binding.
	/// </summary>
	/// <remarks>
	/// Derived classes can easily add new properties using the inherited <see cref="Update"/> method.
	/// For example, you can add a Comment property like:
	/// <code>
	/// private string comment;
	/// public string Comment { get => this.comment; set => this.Update(ref this.comment, value); }
	/// </code>
	/// </remarks>
	public abstract class PropertyChangeNotifier : INotifyPropertyChanged
	{
		#region Constructors

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		protected PropertyChangeNotifier()
		{
		}

		#endregion

		#region Public Events

		/// <summary>
		/// Raised when a property's value has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Protected Methods

		/// <summary>
		/// Used to update a member variable and raise the <see cref="PropertyChanged"/> event if necessary.
		/// </summary>
		/// <typeparam name="T">The data type of the member being updated.</typeparam>
		/// <param name="member">A reference to the member being updated.</param>
		/// <param name="value">The new value to store in the member variable.</param>
		/// <param name="callerMemberName">The name of the caller. Normally, you shouldn't pass anything
		/// for this parameter. If omitted, the C# compiler will inject the caller's property name automatically
		/// using the <see cref="CallerMemberNameAttribute"/>.</param>
		protected void Update<T>(ref T member, T value, [CallerMemberName] string callerMemberName = null)
		{
			if (!EqualityComparer<T>.Default.Equals(member, value))
			{
				member = value;
				PropertyChangedEventHandler handler = this.PropertyChanged;
				if (handler != null)
				{
					PropertyChangedEventArgs args = new PropertyChangedEventArgs(callerMemberName);
					handler(this, args);
				}
			}
		}

		#endregion
	}
}
