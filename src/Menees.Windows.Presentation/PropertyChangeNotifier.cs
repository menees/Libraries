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
		/// <returns>True if the member was updated. False if the member wasn't updated.</returns>
		protected bool Update<T>(ref T member, T value, [CallerMemberName] string callerMemberName = null)
		{
			bool result = false;

			if (!EqualityComparer<T>.Default.Equals(member, value))
			{
				member = value;
				result = true;
				this.OnPropertyChanged(callerMemberName);
			}

			return result;
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">The name of the property that has changed.</param>
		/// <remarks>
		/// This method is useful when setting one property via <see cref="Update"/>
		/// also causes a dependent "read-only" property to be updated.
		/// </remarks>
		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				PropertyChangedEventArgs args = new(propertyName);
				handler(this, args);
			}
		}

		#endregion
	}
}
