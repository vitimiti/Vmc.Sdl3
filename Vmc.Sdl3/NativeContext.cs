using Vmc.Sdl3.Internals;

namespace Vmc.Sdl3;

/// <summary>
/// Represents the native context for the SDL application, managing the initialization and cleanup of SDL subsystems.
/// </summary>
/// <remarks>
/// This class implements <see cref="IDisposable"/>.
/// </remarks>
public sealed class NativeContext : IDisposable
{
    internal NativeContext() => Sdl.UnsafeSetMainReady();

    /// <summary>
    /// Disposes the native context, cleaning up all initialized SDL subsystems and quitting SDL.
    /// </summary>
    public void Dispose()
    {
        foreach (var flag in Enum.GetValues<Sdl.InitFlags>())
        {
            if ((int)Sdl.UnsafeWasInit(flag) != 0)
            {
                Sdl.UnsafeQuitSubSystem(flag);
            }
        }

        Sdl.UnsafeQuit();
    }
}
