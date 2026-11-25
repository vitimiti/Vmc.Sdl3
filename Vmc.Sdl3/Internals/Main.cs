using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vmc.Sdl3.Internals;

internal static partial class Sdl
{
    [LibraryImport(DllName, EntryPoint = "SDL_SetMainReady")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void UnsafeSetMainReady();
}
