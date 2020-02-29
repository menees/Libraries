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

		private static void ExecuteCommand(object dte, string command, string arg)
		{
			// .NET Core 3.x doesn't support dynamic for COM Interop.
			// https://github.com/dotnet/runtime/issues/30502#issuecomment-518748077
			// To get around that limitation we have to use reflection, at least until .NET 5.
			// https://github.com/dotnet/runtime/issues/12587#issuecomment-585591984
			// https://github.com/dotnet/runtime/issues/12587#issuecomment-534611966
			dynamic comDte = new COMObject(dte);
			comDte.ExecuteCommand(command, arg);
		}

		private static IntPtr GetMainWindowHandle(object dte)
		{
			dynamic comDte = new COMObject(dte);
			dynamic mainWindow = comDte.MainWindow;
			IntPtr result = (IntPtr)Convert.ToInt64(mainWindow.HWnd);
			return result;
		}

		private static void ActivateMainWindow(object dte)
		{
			dynamic comDte = new COMObject(dte);
			dynamic mainWindow = comDte.MainWindow;
			mainWindow.Activate();
			mainWindow.Visible = true;
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
