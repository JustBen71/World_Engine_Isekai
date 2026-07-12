using Isekai.Engine.Core.Component;

namespace Isekai.Engine.Sandbox.Components;

/// <summary>
/// Stores the terminal symbol used to render a sandbox entity.
/// </summary>
public sealed record DisplayGlyphComponent(string Glyph) : IComponent
{
    /// <summary>
    /// Creates a terminal display glyph.
    /// </summary>
    public DisplayGlyphComponent(char glyph)
        : this(glyph.ToString())
    {
    }
}
