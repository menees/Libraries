namespace Menees.Windows
{
	#region Using Directives

	using System;
	using System.Dynamic;
	using System.Reflection;

	#endregion

	public static partial class VisualStudioInvoker
	{
		#region Private Methods

		private static object GetActiveObject(string progID) => NativeMethods.GetActiveObject(progID);

		private static object EnsureDynamic(object value)
		{
			// .NET Core 3.x doesn't support dynamic for COM Interop.
			// https://github.com/dotnet/runtime/issues/30502#issuecomment-518748077
			// To get around that limitation until .NET 5, we have to use a DynamicObject.
			// https://github.com/dotnet/runtime/issues/12587#issuecomment-585591984
			// https://github.com/dotnet/runtime/issues/12587#issuecomment-534611966
			dynamic result = new COMObject(value);
			return result;
		}

		#endregion

		#region Private Types

		// https://github.com/dotnet/runtime/issues/12587#issuecomment-534611966
		// A small wrapper around COM interop to make it more easy to use.
		private class COMObject : DynamicObject
		{
			#region Private Data Members

			private readonly object instance;

			#endregion

			#region Constructors

			public COMObject(object instance)
			{
				this.instance = instance;
			}

			#endregion

			#region Public Methods

			public static COMObject CreateObject(string progID)
			{
				return new COMObject(Activator.CreateInstance(Type.GetTypeFromProgID(progID, true)));
			}

			public override bool TryGetMember(GetMemberBinder binder, out object result)
			{
				result = this.instance.GetType().InvokeMember(
					binder.Name,
					BindingFlags.GetProperty,
					Type.DefaultBinder,
					this.instance,
					Array.Empty<object>());
				result = WrapIfRequired(result);
				return true;
			}

			public override bool TrySetMember(SetMemberBinder binder, object value)
			{
				this.instance.GetType().InvokeMember(
					binder.Name,
					BindingFlags.SetProperty,
					Type.DefaultBinder,
					this.instance,
					new object[] { value });
				return true;
			}

			public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
			{
				result = this.instance.GetType().InvokeMember(
					binder.Name,
					BindingFlags.InvokeMethod,
					Type.DefaultBinder,
					this.instance,
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
					result = new COMObject(obj);
				}

				return result;
			}

			#endregion
		}

		#endregion
	}
}
