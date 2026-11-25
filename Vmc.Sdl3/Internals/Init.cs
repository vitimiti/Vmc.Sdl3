using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vmc.Sdl3.Internals;

internal static unsafe partial class Sdl
{
    public static ReadOnlySpan<byte> PropAppMetadataNameString => "SDL.app.metadata.name"u8;
    public static ReadOnlySpan<byte> PropAppMetadataVersionString => "SDL.app.metadata.version"u8;
    public static ReadOnlySpan<byte> PropAppMetadataIdentifierString =>
        "SDL.app.metadata.identifier"u8;
    public static ReadOnlySpan<byte> PropAppMetadataCreatorString => "SDL.app.metadata.creator"u8;
    public static ReadOnlySpan<byte> PropAppMetadataCopyrightString =>
        "SDL.app.metadata.copyright"u8;
    public static ReadOnlySpan<byte> PropAppMetadataUrlString => "SDL.app.metadata.url"u8;
    public static ReadOnlySpan<byte> PropAppMetadataTypeString => "SDL.app.metadata.type"u8;

    [LibraryImport(DllName, EntryPoint = "SDL_InitSubSystem")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeInitSubSystem(InitFlags flags);

    [LibraryImport(DllName, EntryPoint = "SDL_QuitSubSystem")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void UnsafeQuitSubSystem(InitFlags flags);

    [LibraryImport(DllName, EntryPoint = "SDL_WasInit")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial InitFlags UnsafeWasInit(InitFlags flags);

    [LibraryImport(DllName, EntryPoint = "SDL_Quit")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void UnsafeQuit();

    [LibraryImport(DllName, EntryPoint = "SDL_IsMainThread")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeIsMainThread();

    [LibraryImport(DllName, EntryPoint = "SDL_SetAppMetadataPropety")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeSetAppMetadataProperty(byte* name, byte* value);

    [Flags]
    public enum InitFlags : uint
    {
        Audio = 0x0000_0010U,
        Video = 0x0000_0020U,
        Joystick = 0x0000_0200U,
        Haptic = 0x0000_1000U,
        Gamepad = 0x0000_2000U,
        Events = 0x0000_4000U,
        Sensor = 0x0000_8000U,
        Camera = 0x0001_0000U,
    }
}
