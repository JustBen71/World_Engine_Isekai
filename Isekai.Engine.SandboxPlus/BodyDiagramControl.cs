using Isekai.Engine.Core;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Injuries;
using Isekai.Engine.Sandbox.Components;

namespace Isekai.Engine.SandboxPlus;

/// <summary>
/// Draws the selected entity body parts and their current integrity.
/// </summary>
internal sealed class BodyDiagramControl : Control
{
    private readonly Font _titleFont = new("Segoe UI", 13, FontStyle.Bold, GraphicsUnit.Point);
    private readonly Font _labelFont = new("Segoe UI", 9, FontStyle.Regular, GraphicsUnit.Point);
    private readonly Font _percentFont = new("Segoe UI", 9, FontStyle.Bold, GraphicsUnit.Point);
    private WorldState? _world;
    private Entity? _entity;

    public BodyDiagramControl()
    {
        DoubleBuffered = true;
        BackColor = Palette.Panel;
        ForeColor = Palette.Text;
        MinimumSize = new Size(280, 240);
    }

    public void SetEntity(WorldState? world, Entity? entity)
    {
        _world = world;
        _entity = entity;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.Clear(BackColor);

        if (_world is null || _entity is null)
        {
            DrawEmpty(e.Graphics, "Selectionne une entite");
            return;
        }

        if (!_entity.TryGetComponent<BodyStateComponent>(out var body) || body is null)
        {
            DrawEmpty(e.Graphics, "Aucun corps detaille");
            return;
        }

        var name = _entity.TryGetComponent<NameComponent>(out var nameComponent) && nameComponent is not null
            ? nameComponent.Name
            : _entity.Id.ToString();

        using var titleBrush = new SolidBrush(Palette.Text);
        e.Graphics.DrawString($"Corps - {name}", _titleFont, titleBrush, 18, 16);

        var parts = body.Parts.ToDictionary(part => part.PartId, StringComparer.OrdinalIgnoreCase);
        if (LooksHumanoid(parts.Keys))
        {
            DrawHumanoid(e.Graphics, parts);
        }
        else if (LooksQuadruped(parts.Keys))
        {
            DrawQuadruped(e.Graphics, parts);
        }
        else
        {
            DrawGenericParts(e.Graphics, body.Parts);
        }
    }

    private void DrawHumanoid(Graphics graphics, IReadOnlyDictionary<string, BodyPartState> parts)
    {
        var cx = ClientSize.Width / 2;
        DrawPart(graphics, parts, "head", new Rectangle(cx - 34, 56, 68, 52), "Head");
        DrawPart(graphics, parts, "torso", new Rectangle(cx - 42, 116, 84, 88), "Torso");
        DrawPart(graphics, parts, "left_arm", new Rectangle(cx - 116, 124, 58, 78), "L arm");
        DrawPart(graphics, parts, "right_arm", new Rectangle(cx + 58, 124, 58, 78), "R arm");
        DrawPart(graphics, parts, "left_leg", new Rectangle(cx - 60, 214, 52, 86), "L leg");
        DrawPart(graphics, parts, "right_leg", new Rectangle(cx + 8, 214, 52, 86), "R leg");
    }

    private void DrawQuadruped(Graphics graphics, IReadOnlyDictionary<string, BodyPartState> parts)
    {
        var cx = ClientSize.Width / 2;
        DrawPart(graphics, parts, "head", new Rectangle(cx + 72, 92, 62, 50), "Head");
        DrawPart(graphics, parts, "torso", new Rectangle(cx - 74, 112, 132, 72), "Torso");
        DrawPart(graphics, parts, "front_legs", new Rectangle(cx + 30, 190, 64, 84), "Front");
        DrawPart(graphics, parts, "rear_legs", new Rectangle(cx - 80, 190, 64, 84), "Rear");
    }

    private void DrawGenericParts(Graphics graphics, IEnumerable<BodyPartState> parts)
    {
        var x = 20;
        var y = 60;
        foreach (var part in parts.OrderBy(part => part.PartId, StringComparer.OrdinalIgnoreCase))
        {
            DrawPartBox(graphics, part, new Rectangle(x, y, ClientSize.Width - 40, 34), part.PartId);
            y += 42;
        }
    }

    private void DrawPart(Graphics graphics, IReadOnlyDictionary<string, BodyPartState> parts, string id, Rectangle rect, string label)
    {
        if (!parts.TryGetValue(id, out var part))
        {
            return;
        }

        DrawPartBox(graphics, part, rect, label);
    }

    private void DrawPartBox(Graphics graphics, BodyPartState part, Rectangle rect, string label)
    {
        var ratio = part.MaxIntegrity <= 0 ? 0 : Math.Clamp(part.Integrity / part.MaxIntegrity, 0, 1);
        var color = GetIntegrityColor(ratio);
        using var fill = new SolidBrush(Color.FromArgb(44, color));
        using var border = new Pen(color, 2);
        using var text = new SolidBrush(Palette.Text);
        using var muted = new SolidBrush(Palette.Muted);

        graphics.FillRoundedRectangle(fill, rect, 10);
        graphics.DrawRoundedRectangle(border, rect, 10);

        var percent = $"{ratio * 100:0}%";
        graphics.DrawString(label, _labelFont, text, rect.X + 8, rect.Y + 6);
        graphics.DrawString(percent, _percentFont, muted, rect.X + 8, rect.Y + rect.Height - 22);

        if (_entity?.TryGetComponent<InjuryComponent>(out var injuries) == true && injuries is not null &&
            injuries.Injuries.Any(injury => string.Equals(injury.BodyPartId, part.PartId, StringComparison.OrdinalIgnoreCase)))
        {
            using var injuryBrush = new SolidBrush(Color.FromArgb(230, 223, 84, 84));
            graphics.FillEllipse(injuryBrush, rect.Right - 17, rect.Y + 9, 8, 8);
        }
    }

    private void DrawEmpty(Graphics graphics, string message)
    {
        using var titleBrush = new SolidBrush(Palette.Text);
        using var mutedBrush = new SolidBrush(Palette.Muted);
        graphics.DrawString("Corps", _titleFont, titleBrush, 18, 16);
        graphics.DrawString(message, _labelFont, mutedBrush, 18, 54);
    }

    private static bool LooksHumanoid(IEnumerable<string> ids)
    {
        var set = ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return set.Contains("head") && set.Contains("torso") && set.Contains("left_arm") && set.Contains("right_arm");
    }

    private static bool LooksQuadruped(IEnumerable<string> ids)
    {
        var set = ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return set.Contains("head") && set.Contains("torso") && set.Contains("front_legs") && set.Contains("rear_legs");
    }

    private static Color GetIntegrityColor(double ratio)
    {
        return ratio switch
        {
            < 0.25 => Color.FromArgb(226, 83, 83),
            < 0.65 => Color.FromArgb(232, 181, 81),
            _ => Color.FromArgb(91, 179, 119)
        };
    }

    private static class Palette
    {
        public static readonly Color Panel = Color.FromArgb(20, 28, 36);
        public static readonly Color Text = Color.FromArgb(230, 237, 243);
        public static readonly Color Muted = Color.FromArgb(142, 159, 171);
    }
}

internal static class RoundedRectangleExtensions
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
    {
        using var path = CreatePath(bounds, radius);
        graphics.FillPath(brush, path);
    }

    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radius)
    {
        using var path = CreatePath(bounds, radius);
        graphics.DrawPath(pen, path);
    }

    private static System.Drawing.Drawing2D.GraphicsPath CreatePath(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var diameter = radius * 2;
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
