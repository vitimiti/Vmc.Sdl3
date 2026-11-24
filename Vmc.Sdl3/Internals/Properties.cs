using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vmc.Sdl3.Properties;

namespace Vmc.Sdl3.Internals;

internal static unsafe partial class Sdl
{
    public static ReadOnlySpan<byte> PropNameString => "SDL.name\0"u8;

    [LibraryImport(DllName, EntryPoint = "SDL_GetGlobalProperties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial PropertiesID UnsafeGetGlobalProperties();

    [LibraryImport(DllName, EntryPoint = "SDL_CreateProperties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial PropertiesID UnsafeCreateProperties();

    [LibraryImport(DllName, EntryPoint = "SDL_CopyProperties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeCopyProperties(PropertiesID src, PropertiesID dst);

    [LibraryImport(DllName, EntryPoint = "SDL_LockProperties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeLockProperties(PropertiesID id);

    [LibraryImport(DllName, EntryPoint = "SDL_UnlockProperties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void UnsafeUnlockProperties(PropertiesID id);

    [LibraryImport(DllName, EntryPoint = "SDL_SetPointerPropertyWithCleanup")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeSetPointerPropertyWithCleanup(
        PropertiesID props,
        byte* name,
        void* value,
        delegate* unmanaged[Cdecl]<void*, void*, void> cleanup,
        void* userdata
    );

    [LibraryImport(DllName, EntryPoint = "SDL_SetPointerProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeSetPointerProperty(
        PropertiesID props,
        byte* name,
        void* value
    );

    [LibraryImport(DllName, EntryPoint = "SDL_SetStringProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeSetStringProperty(PropertiesID props, byte* name, byte* value);

    [LibraryImport(DllName, EntryPoint = "SDL_SetNumberProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeSetNumberProperty(PropertiesID props, byte* name, long value);

    [LibraryImport(DllName, EntryPoint = "SDL_SetFloatProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeSetFloatProperty(PropertiesID props, byte* name, float value);

    [LibraryImport(DllName, EntryPoint = "SDL_SetBooleanProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeSetBooleanProperty(
        PropertiesID props,
        byte* name,
        [MarshalAs(UnmanagedType.I1)] bool value
    );

    [LibraryImport(DllName, EntryPoint = "SDL_HasProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeHasProperty(PropertiesID props, byte* name);

    [LibraryImport(DllName, EntryPoint = "SDL_GetPropertyType")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial PropertyType UnsafeGetPropertyType(PropertiesID props, byte* name);

    [LibraryImport(DllName, EntryPoint = "SDL_GetPointerProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void* UnsafeGetPointerProperty(
        PropertiesID props,
        byte* name,
        void* defaultValue
    );

    [LibraryImport(DllName, EntryPoint = "SDL_GetStringProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial byte* UnsafeGetStringProperty(
        PropertiesID props,
        byte* name,
        byte* defaultValue
    );

    [LibraryImport(DllName, EntryPoint = "SDL_GetNumberProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long UnsafeGetNumberProperty(
        PropertiesID props,
        byte* name,
        long defaultValue
    );

    [LibraryImport(DllName, EntryPoint = "SDL_GetFloatProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial float UnsafeGetFloatProperty(
        PropertiesID props,
        byte* name,
        float defaultValue
    );

    [LibraryImport(DllName, EntryPoint = "SDL_GetBooleanProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeGetBooleanProperty(
        PropertiesID props,
        byte* name,
        [MarshalAs(UnmanagedType.I1)] bool defaultValue
    );

    [LibraryImport(DllName, EntryPoint = "SDL_ClearProperty")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeClearProperty(PropertiesID props, byte* name);

    [LibraryImport(DllName, EntryPoint = "SDL_EnumerateProperties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool UnsafeEnumerateProperties(
        PropertiesID props,
        delegate* unmanaged[Cdecl]<void*, PropertiesID, byte*, void> callback,
        void* userData
    );

    [LibraryImport(DllName, EntryPoint = "SDL_DestroyProperties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void UnsafeDestroyProperties(PropertiesID props);

    public readonly struct PropertiesID(uint value)
    {
        public readonly uint Value = value;

        public static implicit operator PropertiesID(uint value) => new(value);

        public static implicit operator uint(PropertiesID id) => id.Value;
    }
}
