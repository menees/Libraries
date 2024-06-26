// <auto-generated>
// This was generated by ILSpy from the "tom" (Text Object Model) COM typelib used by the Windows RichEdit control.
// These COM interfaces must exactly match their declarations in %ProgramFiles(x86)%\Windows Kits\10\Include\10.0.19041.0\um\TOM.h.
// We're using this generated C# code because "dotnet build" doesn't support a COMReference in .NET "Core".
// https://learn.microsoft.com/en-us/dotnet/framework/interop/how-to-create-wrappers-manually
// https://learn.microsoft.com/en-us/windows/win32/api/tom/nn-tom-itextdocument
// https://learn.microsoft.com/en-us/visualstudio/msbuild/resolvecomreference-task#msb4803-error
// </auto-generated>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member. Microsoft defines this COM API.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace tom;

[ComImport]
[TypeLibType(4288)]
[DefaultMember("Name")]
[Guid("8CC497C0-A1DF-11CE-8098-00AA0047BE5D")]
public interface ITextDocument
{
	[DispId(0)]
	string Name
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(0)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1)]
	ITextSelection Selection
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(2)]
	int StoryCount
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		get;
	}

	[DispId(3)]
	ITextStoryRanges StoryRanges
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(3)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(4)]
	int Saved
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(4)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(4)]
		[param: In]
		set;
	}

	[DispId(5)]
	float DefaultTabStop
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(5)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(5)]
		[param: In]
		set;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	void New();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(7)]
	void Open([In][MarshalAs(UnmanagedType.Struct)] ref object pVar, [In] int Flags, [In] int CodePage);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(8)]
	void Save([In][MarshalAs(UnmanagedType.Struct)] ref object pVar, [In] int Flags, [In] int CodePage);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(9)]
	int Freeze();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(10)]
	int Unfreeze();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(11)]
	void BeginEditCollection();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(12)]
	void EndEditCollection();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(13)]
	int Undo([In] int Count);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(14)]
	int Redo([In] int Count);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(15)]
	[return: MarshalAs(UnmanagedType.Interface)]
	ITextRange Range([In] int cp1, [In] int cp2);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(16)]
	[return: MarshalAs(UnmanagedType.Interface)]
	ITextRange RangeFromPoint([In] int x, [In] int y);
}
