using System.Runtime.InteropServices;

namespace Vmc.Sdl3.Internals;

internal static partial class Sdl
{
    private const string DllName = "SDL3";

    static Sdl() =>
        NativeLibrary.SetDllImportResolver(
            typeof(Sdl).Assembly,
            (name, assembly, path) =>
                NativeLibrary.Load(
                    name switch
                    {
                        DllName when OperatingSystem.IsWindows() => "SDL3.dll",
                        DllName when OperatingSystem.IsLinux() => "libSDL3.so.0",
                        DllName when OperatingSystem.IsMacOS() => "libSDL3.0.dylib",
                        _ => name,
                    }
                )
        );
}
