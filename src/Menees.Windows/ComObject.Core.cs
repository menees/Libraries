namespace Menees.Windows
{
	#region Using Directives

	using System;
	using System.Dynamic;
	using System.Reflection;

	#endregion

	// https://github.com/dotnet/runtime/issues/12587#issuecomment-534611966
	// A small wrapper around COM interop to make it more easy to use.
	internal class ComObject : DynamicObject
	{
		#region Constructors

		public ComObject(object instance)
		{
			this.Instance = instance;
		}

		#endregion

		#region Public Properties

		public object Instance { get; }

		#endregion

		#region Public Methods

		public static ComObject CreateObject(string progID)
		{
			return new ComObject(Activator.CreateInstance(Type.GetTypeFromProgID(progID, true)));
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = this.Instance.GetType().InvokeMember(
				binder.Name,
				BindingFlags.GetProperty,
				Type.DefaultBinder,
				this.Instance,
				Array.Empty<object>());
			result = WrapIfRequired(result);
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			this.Instance.GetType().InvokeMember(
				binder.Name,
				BindingFlags.SetProperty,
				Type.DefaultBinder,
				this.Instance,
				new object[] { value });
			return true;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = this.Instance.GetType().InvokeMember(
				binder.Name,
				BindingFlags.InvokeMethod,
				Type.DefaultBinder,
				this.Instance,
				args);
			result = WrapIfRequired(result);
			return true;
		}

		#endregion

		#region Private Methods

		// https://github.com/dotnet/runtime/issues/12587#issuecomment-578431424
		private static object WrapIfRequired(object obj)
		{
			object result = obj;

			if (obj != null && !obj.GetType().IsPrimitive)
			{
				result = new ComObject(obj);
			}

			return result;
		}

		#endregion
	}
}
