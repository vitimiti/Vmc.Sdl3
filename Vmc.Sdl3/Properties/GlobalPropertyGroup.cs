using Vmc.Sdl3.Internals;

namespace Vmc.Sdl3.Properties;

/// <summary>
/// Represents the global property group.
/// </summary>
/// <remarks>
/// This class implements <see cref="PropertyGroup"/> and shares its functionality.
/// </remarks>
public sealed class GlobalPropertyGroup : PropertyGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalPropertyGroup"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the global property group could not be obtained.</exception>
    public GlobalPropertyGroup()
        : base(Sdl.UnsafeGetGlobalProperties(), ownsProperties: false)
    {
        if (ID == 0)
        {
            throw new InvalidOperationException(
                $"Failed to get global property group: {Sdl.GetError()}."
            );
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalPropertyGroup"/> class with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the global property group.</param>
    internal GlobalPropertyGroup(Sdl.PropertiesID id)
        : base(id, ownsProperties: false) { }

    /// <summary>
    /// Attempts to get the global property group.
    /// </summary>
    /// <param name="group">The global property group if successful; otherwise, null.</param>
    /// <param name="error">The error message if the global property group could not be obtained; otherwise, null.</param>
    /// <returns><see langword="true" /> if the global property group was obtained successfully; otherwise, <see langword="false" />.</returns>
    public static bool TryGetGlobalPropertyGroup(out GlobalPropertyGroup? group, out string? error)
    {
        var id = Sdl.UnsafeGetGlobalProperties();
        if (id == 0)
        {
            error = Sdl.GetError();
            group = null;
            return false;
        }

        error = null;
        group = new GlobalPropertyGroup(id);
        return true;
    }
}
