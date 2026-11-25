using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Vmc.Sdl3.Internals;

namespace Vmc.Sdl3.Properties;

/// <summary>
/// Represents a group of properties.
/// </summary>
/// <remarks>
/// This class implements <see cref="IDisposable"/> to allow proper cleanup of unmanaged resources.
/// </remarks>
public class PropertyGroup : IDisposable
{
    /// <summary>
    /// Represents a callback for cleaning up a pointer property.
    /// </summary>
    /// <param name="value">The pointer value to clean up.</param>
    public delegate void PointerCleanupCallback(nint value);

    /// <summary>
    /// Represents a callback for enumerating properties.
    /// </summary>
    /// <param name="name">The name of the property being enumerated.</param>
    public delegate void EnumeratePropertiesCallback(string name);

    private static readonly Callbacks CallbacksInstances = new();

    private static GCHandle _callbacksHandle;

    private readonly bool _ownsProperties;

    private bool _disposed;

    /// <summary>
    /// Gets the ID of the property group.
    /// </summary>
    internal Sdl.PropertiesID ID { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="PropertyGroup"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the property group could not be created.</exception>
    public PropertyGroup()
    {
        ID = Sdl.UnsafeCreateProperties();
        if (ID == 0)
        {
            throw new InvalidOperationException(
                $"Failed to create a new property group: {Sdl.GetError()}."
            );
        }

        AllocateCallbacks();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PropertyGroup"/> class with the specified ID and ownership flag.
    /// </summary>
    /// <param name="id">The ID of the property group.</param>
    /// <param name="ownsProperties">Indicates whether this instance owns the properties and is responsible for their cleanup.</param>
    internal PropertyGroup(Sdl.PropertiesID id, bool ownsProperties = true)
    {
        ID = id;
        _ownsProperties = ownsProperties;
        AllocateCallbacks();
    }

    /// <summary>
    /// Finalizes the property group and releases its resources.
    /// </summary>
    ~PropertyGroup()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Attempts to create a new property group.
    /// </summary>
    /// <param name="group">The created property group if successful; otherwise, null.</param>
    /// <param name="error">The error message if creation failed; otherwise, null.</param>
    /// <returns><see langword="true" /> if the property group was created successfully; otherwise, <see langword="false" />.</returns>
    public static bool TryCreatePropertyGroup(out PropertyGroup? group, out string? error)
    {
        var id = Sdl.UnsafeCreateProperties();
        if (id == 0)
        {
            error = Sdl.GetError();
            group = null;
            return false;
        }

        error = null;
        group = new PropertyGroup(id);
        return true;
    }

    /// <summary>
    /// Disposes the property group and releases its resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the resources used by the property group.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> if called from the Dispose method; <see langword="false"/> if called from the finalizer.</param>
    /// <remarks>
    /// Override this method to release additional resources if needed, but make sure to always call <c>base.Dispose(disposing)</c>.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_callbacksHandle.IsAllocated)
        {
            _callbacksHandle.Free();
        }

        if (_ownsProperties)
        {
            Sdl.UnsafeDestroyProperties(ID);
        }
    }

    /// <summary>
    /// Copies the properties from this property group to the specified target property group.
    /// </summary>
    /// <param name="target">The target property group to copy properties to.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the copy operation fails.</exception>
    public void CopyPropertiesTo(PropertyGroup target)
    {
        ThrowIfDisposed();
        if (!Sdl.UnsafeCopyProperties(ID, target.ID))
        {
            throw new InvalidOperationException($"Failed to copy properties: {Sdl.GetError()}.");
        }
    }

    /// <summary>
    /// Attempts to copy the properties from this property group to the specified target property group.
    /// </summary>
    /// <param name="target">The target property group to copy properties to.</param>
    /// <param name="error">The error message if the copy operation fails; otherwise, null.</param>
    /// <returns><see langword="true"/> if the properties were copied successfully; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public bool TryCopyPropertiesTo(PropertyGroup target, out string? error)
    {
        ThrowIfDisposed();
        if (!Sdl.UnsafeCopyProperties(ID, target.ID))
        {
            error = Sdl.GetError();
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Locks the properties of this property group, preventing further modifications until the lock is released.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that releases the lock when disposed.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the lock operation fails.</exception>
    public IDisposable Lock()
    {
        ThrowIfDisposed();
        return !Sdl.UnsafeLockProperties(ID)
            ? throw new InvalidOperationException($"Failed to lock properties: {Sdl.GetError()}.")
            : (IDisposable)new LockedPropertyGroup(this);
    }

    /// <summary>
    /// Sets a pointer property for this property group with an optional cleanup callback.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The pointer value to set for the property.</param>
    /// <param name="cleanup">An optional cleanup callback to be invoked when the property is no longer needed.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if setting the property fails.</exception>
    public unsafe void SetProperty(string name, nint value, PointerCleanupCallback? cleanup)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            if (cleanup is null)
            {
                CallbacksInstances.PointerCleanup = null;
                if (!Sdl.UnsafeSetPointerPropertyWithCleanup(ID, namePtr, (void*)value, null, null))
                {
                    throw new InvalidOperationException(
                        $"Failed to set pointer property '{name}' to '0x{value:x16}': {Sdl.GetError()}."
                    );
                }
            }
            else
            {
                CallbacksInstances.PointerCleanup = cleanup;
                if (
                    !Sdl.UnsafeSetPointerPropertyWithCleanup(
                        ID,
                        namePtr,
                        (void*)value,
                        &PropertyPointerCleanupHandler,
                        (void*)GCHandle.ToIntPtr(_callbacksHandle)
                    )
                )
                {
                    throw new InvalidOperationException(
                        $"Failed to set pointer property '{name}' to '0x{value:x16}': {Sdl.GetError()}."
                    );
                }
            }
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Tries to set a pointer property for this property group with an optional cleanup callback.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The pointer value to set for the property.</param>
    /// <param name="cleanup">An optional cleanup callback to be invoked when the property is no longer needed.</param>
    /// <param name="error">Outputs the error message if setting the property fails.</param>
    /// <returns><see langword="true" /> if the property was set successfully; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public bool TrySetProperty(
        string name,
        nint value,
        PointerCleanupCallback? cleanup,
        out string? error
    )
    {
        try
        {
            SetProperty(name, value, cleanup);
            error = null;
            return true;
        }
        catch (InvalidOperationException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Sets a pointer property for this property group.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The pointer value to set for the property.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if setting the property fails.</exception>
    public unsafe void SetProperty(string name, nint value)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            if (!Sdl.UnsafeSetPointerProperty(ID, namePtr, (void*)value))
            {
                throw new InvalidOperationException(
                    $"Failed to set pointer property '{name}' to '0x{value:x16}': {Sdl.GetError()}."
                );
            }
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Tries to set a pointer property for this property group.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The pointer value to set for the property.</param>
    /// <param name="error">Outputs the error message if setting the property fails.</param>
    /// <returns><see langword="true" /> if the property was set successfully; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public bool TrySetProperty(string name, nint value, out string? error)
    {
        try
        {
            SetProperty(name, value);
            error = null;
            return true;
        }
        catch (InvalidOperationException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Sets a string property for this property group.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The string value to set for the property.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if setting the property fails.</exception>
    public unsafe void SetProperty(string name, string? value)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        var valuePtr = value is null ? null : (byte*)Marshal.StringToCoTaskMemUTF8(value);
        try
        {
            if (!Sdl.UnsafeSetStringProperty(ID, namePtr, valuePtr))
            {
                throw new InvalidOperationException(
                    $"Failed to set string property '{name}' to '{value}': {Sdl.GetError()}."
                );
            }
        }
        finally
        {
            if (valuePtr is not null)
            {
                Marshal.FreeCoTaskMem((nint)valuePtr);
            }

            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Tries to set a string property for this property group.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The string value to set for the property.</param>
    /// <param name="error">Outputs the error message if setting the property fails.</param>
    /// <returns><see langword="true" /> if the property was set successfully; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public bool TrySetProperty(string name, string? value, out string? error)
    {
        try
        {
            SetProperty(name, value);
            error = null;
            return true;
        }
        catch (InvalidOperationException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Sets a number property for this property group.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The number value to set for the property.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if setting the property fails.</exception>
    public unsafe void SetProperty(string name, long value)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            if (!Sdl.UnsafeSetNumberProperty(ID, namePtr, value))
            {
                throw new InvalidOperationException(
                    $"Failed to set number property '{name}' to '{value}': {Sdl.GetError()}."
                );
            }
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Tries to set a number property for this property group.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The number value to set for the property.</param>
    /// <param name="error">Outputs the error message if setting the property fails.</param>
    /// <returns><see langword="true" /> if the property was set successfully; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public bool TrySetProperty(string name, long value, out string? error)
    {
        try
        {
            SetProperty(name, value);
            error = null;
            return true;
        }
        catch (InvalidOperationException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Sets a float property for this property group.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The float value to set for the property.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if setting the property fails.</exception>
    public unsafe void SetProperty(string name, float value)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            if (!Sdl.UnsafeSetFloatProperty(ID, namePtr, value))
            {
                throw new InvalidOperationException(
                    $"Failed to set float property '{name}' to '{value}': {Sdl.GetError()}."
                );
            }
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Tries to set a float property for this property group.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The float value to set for the property.</param>
    /// <param name="error">Outputs the error message if setting the property fails.</param>
    /// <returns><see langword="true" /> if the property was set successfully; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public bool TrySetProperty(string name, float value, out string? error)
    {
        try
        {
            SetProperty(name, value);
            error = null;
            return true;
        }
        catch (InvalidOperationException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Sets a boolean property for this property group.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The boolean value to set for the property.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if setting the property fails.</exception>
    public unsafe void SetProperty(string name, bool value)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            if (!Sdl.UnsafeSetBooleanProperty(ID, namePtr, value))
            {
                throw new InvalidOperationException(
                    $"Failed to set boolean property '{name}' to '{value}': {Sdl.GetError()}."
                );
            }
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Tries to set a boolean property for this property group.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The boolean value to set for the property.</param>
    /// <param name="error">Outputs the error message if setting the property fails.</param>
    /// <returns><see langword="true" /> if the property was set successfully; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public bool TrySetProperty(string name, bool value, out string? error)
    {
        try
        {
            SetProperty(name, value);
            error = null;
            return true;
        }
        catch (InvalidOperationException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Checks if this property group has a property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to check.</param>
    /// <returns><see langword="true" /> if the property exists; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public unsafe bool HasProperty(string name)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            return Sdl.UnsafeHasProperty(ID, namePtr);
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Gets the type of the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <returns>The type of the property.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public unsafe PropertyType GetPropertyType(string name)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            return Sdl.UnsafeGetPropertyType(ID, namePtr);
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Gets a pointer property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="defaultValue">The default value to return if the property is not set.</param>
    /// <returns>The pointer value of the property.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public unsafe nint GetPointerProperty(string name, nint defaultValue = 0)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            return (nint)Sdl.UnsafeGetPointerProperty(ID, namePtr, (void*)defaultValue);
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Gets a string property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="defaultValue">The default value to return if the property is not set.</param>
    /// <returns>The string value of the property.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public unsafe string? GetStringProperty(string name, string? defaultValue = null)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        var defaultValuePtr = defaultValue is null
            ? null
            : (byte*)Marshal.StringToCoTaskMemUTF8(defaultValue);
        try
        {
            return Marshal.PtrToStringUTF8(
                (nint)Sdl.UnsafeGetStringProperty(ID, namePtr, defaultValuePtr)
            );
        }
        finally
        {
            if (defaultValuePtr is not null)
            {
                Marshal.FreeCoTaskMem((nint)defaultValuePtr);
            }

            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Gets a numeric property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="defaultValue">The default value to return if the property is not set.</param>
    /// <returns>The numeric value of the property.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public unsafe long GetNumberProperty(string name, long defaultValue = 0)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            return Sdl.UnsafeGetNumberProperty(ID, namePtr, defaultValue);
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Gets a float property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="defaultValue">The default value to return if the property is not set.</param>
    /// <returns>The float value of the property.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public unsafe float GetFloatProperty(string name, float defaultValue = 0)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            return Sdl.UnsafeGetFloatProperty(ID, namePtr, defaultValue);
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Gets a boolean property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="defaultValue">The default value to return if the property is not set.</param>
    /// <returns>The boolean value of the property.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public unsafe bool GetBooleanProperty(string name, bool defaultValue = false)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            return Sdl.UnsafeGetBooleanProperty(ID, namePtr, defaultValue);
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Clears the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the property could not be cleared.</exception>
    public unsafe void ClearProperty(string name)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(name);
        var namePtr = (byte*)Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            if (!Sdl.UnsafeClearProperty(ID, namePtr))
            {
                throw new InvalidOperationException(
                    $"Failed to clear property '{name}': {Sdl.GetError()}."
                );
            }
        }
        finally
        {
            Marshal.FreeCoTaskMem((nint)namePtr);
        }
    }

    /// <summary>
    /// Tries to clear the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="error">The error message if the property could not be cleared.</param>
    /// <returns><see langword="true" /> if the property was cleared successfully; otherwise, <see langword="false" />.</returns>
    public bool TryClearProperty(string name, out string? error)
    {
        try
        {
            ClearProperty(name);
            error = null;
            return true;
        }
        catch (InvalidOperationException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Enumerates all properties in the property group.
    /// </summary>
    /// <param name="callback">The callback to invoke for each property.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the properties could not be enumerated.</exception>
    public unsafe void EnumerateProperties(EnumeratePropertiesCallback callback)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(callback);
        if (
            !Sdl.UnsafeEnumerateProperties(
                ID,
                &EnumeratePropertiesHandler,
                (void*)GCHandle.ToIntPtr(_callbacksHandle)
            )
        )
        {
            throw new InvalidOperationException(
                $"Failed to enumerate properties: {Sdl.GetError()}."
            );
        }
    }

    /// <summary>
    /// Tries to enumerate all properties in the property group.
    /// </summary>
    /// <param name="callback">The callback to invoke for each property.</param>
    /// <param name="error">The error message if the properties could not be enumerated.</param>
    /// <returns><see langword="true" /> if the properties were enumerated successfully; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public bool TryEnumerateProperties(EnumeratePropertiesCallback callback, out string? error)
    {
        try
        {
            EnumerateProperties(callback);
            error = null;
            return true;
        }
        catch (InvalidOperationException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Sets the name of the property group.
    /// </summary>
    /// <param name="value">The name to set.</param>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the name could not be set.</exception>
    public void SetName(string? value)
    {
        ThrowIfDisposed();
        SetProperty(Encoding.UTF8.GetString(Sdl.PropNameString), value);
    }

    /// <summary>
    /// Tries to set the name of the property group.
    /// </summary>
    /// <param name="value">The name to set.</param>
    /// <param name="error">The error message if the name could not be set.</param>
    /// <returns><see langword="true" /> if the name was set successfully; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this property group has been disposed.</exception>
    public bool TrySetName(string? value, out string? error)
    {
        try
        {
            SetName(value);
            error = null;
            return true;
        }
        catch (InvalidOperationException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static void AllocateCallbacks() =>
        _callbacksHandle = GCHandle.Alloc(CallbacksInstances);

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void PropertyPointerCleanupHandler(void* userdata, void* value)
    {
        if (userdata is null)
        {
            return;
        }

        try
        {
            var handle = GCHandle.FromIntPtr((nint)userdata);
            if (handle.Target is not Callbacks callbacks)
            {
                return;
            }

            callbacks.PointerCleanup?.Invoke((nint)value);
        }
        catch
        {
            // Ignore exceptions during unmanaged calls
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void EnumeratePropertiesHandler(
        void* userdata,
        Sdl.PropertiesID props,
        byte* name
    )
    {
        if (userdata is null)
        {
            return;
        }

        var handle = GCHandle.FromIntPtr((nint)userdata);
        if (handle.Target is not Callbacks callbacks)
        {
            return;
        }

        callbacks.EnumerateProperties?.Invoke(Marshal.PtrToStringUTF8((nint)name) ?? string.Empty);
    }

    private sealed class LockedPropertyGroup(PropertyGroup group) : IDisposable
    {
        private bool _disposed;

        public PropertyGroup Group { get; } = group;

        internal bool Unlocked { get; private set; } = false;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            try
            {
                Sdl.UnsafeUnlockProperties(Group.ID);
                Unlocked = true;
            }
            catch
            {
                // Ignore exceptions during unlock
                Unlocked = false;
            }
        }
    }

    private sealed class Callbacks
    {
        public PointerCleanupCallback? PointerCleanup { get; set; }
        public EnumeratePropertiesCallback? EnumerateProperties { get; set; }
    }
}
