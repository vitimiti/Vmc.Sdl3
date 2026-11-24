using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vmc.Sdl3.Internals;

internal static unsafe partial class Sdl
{
    public static string? GetError() => Marshal.PtrToStringUTF8((nint)UnsafeGetError());

    [LibraryImport(DllName, EntryPoint = "SDL_GetError")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte* UnsafeGetError();
}
