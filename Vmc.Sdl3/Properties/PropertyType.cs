namespace Vmc.Sdl3.Properties;

/// <summary>
/// Represents the type of a property.
/// </summary>
public enum PropertyType
{
    /// <summary>
    /// Represents an invalid property type.
    /// </summary>
    Invalid,

    /// <summary>
    /// Represents a pointer property type.
    /// </summary>
    Pointer,

    /// <summary>
    /// Represents a string property type.
    /// </summary>
    String,

    /// <summary>
    /// Represents a number property type.
    /// </summary>
    Number,

    /// <summary>
    /// Represents a float property type.
    /// </summary>
    Float,

    /// <summary>
    /// Represents a boolean property type.
    /// </summary>
    Boolean,
}
