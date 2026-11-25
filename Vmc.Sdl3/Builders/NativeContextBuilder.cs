using System.Runtime.InteropServices;
using System.Text;
using Vmc.Sdl3.Internals;

namespace Vmc.Sdl3.Builders;

/// <summary>
/// Builder class for creating and initializing a <see cref="NativeContext"/> with specified SDL subsystems and application metadata.
/// </summary>
public sealed class NativeContextBuilder
{
    private Sdl.InitFlags _initFlags;
    private readonly Dictionary<byte[], string?> _appMetadata = [];

    /// <summary>
    /// Adds the audio subsystem to the SDL initialization flags.
    /// </summary>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithAudioSubsystem()
    {
        _initFlags |= Sdl.InitFlags.Audio;
        return this;
    }

    /// <summary>
    /// Adds the video subsystem to the SDL initialization flags.
    /// </summary>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithVideoSubsystem()
    {
        _initFlags |= Sdl.InitFlags.Video;
        return this;
    }

    /// <summary>
    /// Adds the joystick subsystem to the SDL initialization flags.
    /// </summary>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithJoystickSubsystem()
    {
        _initFlags |= Sdl.InitFlags.Joystick;
        return this;
    }

    /// <summary>
    /// Adds the gamepad subsystem to the SDL initialization flags.
    /// </summary>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithGamepadSubsystem()
    {
        _initFlags |= Sdl.InitFlags.Gamepad;
        return this;
    }

    /// <summary>
    /// Adds the haptic subsystem to the SDL initialization flags.
    /// </summary>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithHapticSubsystem()
    {
        _initFlags |= Sdl.InitFlags.Haptic;
        return this;
    }

    /// <summary>
    /// Adds the events subsystem to the SDL initialization flags.
    /// </summary>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithEventsSubsystem()
    {
        _initFlags |= Sdl.InitFlags.Events;
        return this;
    }

    /// <summary>
    /// Adds the sensor subsystem to the SDL initialization flags.
    /// </summary>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithSensorSubsystem()
    {
        _initFlags |= Sdl.InitFlags.Sensor;
        return this;
    }

    /// <summary>
    /// Adds the camera subsystem to the SDL initialization flags.
    /// </summary>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithCameraSubsystem()
    {
        _initFlags |= Sdl.InitFlags.Camera;
        return this;
    }

    /// <summary>
    /// Sets the name metadata for the application.
    /// </summary>
    /// <param name="name">The name of the application.</param>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithNameMetadata(string? name)
    {
        _appMetadata[Sdl.PropAppMetadataNameString.ToArray()] = name;
        return this;
    }

    /// <summary>
    /// Sets the version metadata for the application.
    /// </summary>
    /// <param name="version">The version of the application.</param>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithVersionMetadata(string? version)
    {
        _appMetadata[Sdl.PropAppMetadataVersionString.ToArray()] = version;
        return this;
    }

    /// <summary>
    /// Sets the identifier metadata for the application.
    /// </summary>
    /// <param name="identifier">The identifier of the application.</param>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithIdentifierMetadata(string? identifier)
    {
        _appMetadata[Sdl.PropAppMetadataIdentifierString.ToArray()] = identifier;
        return this;
    }

    /// <summary>
    /// Sets the creator metadata for the application.
    /// </summary>
    /// <param name="creator">The creator of the application.</param>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithCreatorMetadata(string? creator)
    {
        _appMetadata[Sdl.PropAppMetadataCreatorString.ToArray()] = creator;
        return this;
    }

    /// <summary>
    /// Sets the copyright metadata for the application.
    /// </summary>
    /// <param name="copyright">The copyright of the application.</param>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithCopyrightMetadata(string? copyright)
    {
        _appMetadata[Sdl.PropAppMetadataCopyrightString.ToArray()] = copyright;
        return this;
    }

    /// <summary>
    /// Sets the URL metadata for the application.
    /// </summary>
    /// <param name="url">The URL of the application.</param>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithUrlMetadata(Uri? url)
    {
        _appMetadata[Sdl.PropAppMetadataUrlString.ToArray()] = url?.ToString();
        return this;
    }

    /// <summary>
    /// Sets the type metadata for the application.
    /// </summary>
    /// <param name="type">The type of the application.</param>
    /// <returns>The current instance of <see cref="NativeContextBuilder"/> for method chaining.</returns>
    public NativeContextBuilder WithTypeMetadata(string? type)
    {
        _appMetadata[Sdl.PropAppMetadataTypeString.ToArray()] = type;
        return this;
    }

    /// <summary>
    /// Builds the native context for the application, initializing SDL with the specified subsystems and setting the application metadata.
    /// </summary>
    /// <returns>The built <see cref="NativeContext"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if SDL fails to initialize with the specified subsystems or metadata.</exception>
    public NativeContext Build()
    {
        InitAppMetadata();
        return !Sdl.UnsafeInitSubSystem(_initFlags)
            ? throw new InvalidOperationException(
                $"Failed to initialize SDL with subsystems {_initFlags}: {Sdl.GetError()}."
            )
            : new NativeContext();
    }

    /// <summary>
    /// Attempts to build the native context for the application, capturing any errors that occur during the process.
    /// </summary>
    /// <param name="context">The built <see cref="NativeContext"/> instance if successful; otherwise, null.</param>
    /// <param name="error">The error message if the build fails; otherwise, null.</param>
    /// <returns><see langword="true"/> if the build succeeds; otherwise, <see langword="false"/>.</returns>
    public bool TryBuild(out NativeContext? context, out string? error)
    {
        try
        {
            context = Build();
            error = null;
            return true;
        }
        catch (InvalidOperationException ex)
        {
            context = null;
            error = ex.Message;
            return false;
        }
    }

    private unsafe void InitAppMetadata()
    {
        foreach (var kvp in _appMetadata)
        {
            var valuePtr = (byte*)Marshal.StringToCoTaskMemUTF8(kvp.Value);
            try
            {
                fixed (byte* keyPtr = kvp.Key)
                {
                    if (!Sdl.UnsafeSetAppMetadataProperty(keyPtr, valuePtr))
                    {
                        throw new InvalidOperationException(
                            $"Failed to set app metadata for key {Encoding.UTF8.GetString(kvp.Key)}: {Sdl.GetError()}."
                        );
                    }
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem((IntPtr)valuePtr);
            }
        }
    }
}
