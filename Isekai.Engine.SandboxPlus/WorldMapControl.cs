using Isekai.Engine.Core;
using Isekai.Engine.Modules.Body;
using Isekai.Engine.Modules.Needs;
using Isekai.Engine.Modules.Perception;
using Isekai.Engine.Sandbox.Components;
using Isekai.Engine.Sandbox.World;

namespace Isekai.Engine.SandboxPlus;

/// <summary>
/// Draws a compact colored sandbox map without terminal glyphs.
/// </summary>
internal sealed class WorldMapControl : Control
{
    private readonly Font _entityFont = new("Segoe UI", 11, FontStyle.Bold, GraphicsUnit.Point);
    private readonly Font _emojiFont = new("Segoe UI Emoji", 14, FontStyle.Regular, GraphicsUnit.Point);
    private SandboxSession? _session;
    private EntityId? _selectedEntityId;

    public WorldMapControl()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(18, 24, 30);
        ForeColor = Color.White;
        MinimumSize = new Size(480, 240);
    }

    public void SetSession(SandboxSession? session, EntityId? selectedEntityId)
    {
        _session = session;
        _selectedEntityId = selectedEntityId;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.Clear(BackColor);

        if (_session is null)
        {
            DrawCenteredText(e.Graphics, "Aucun scenario charge");
            return;
        }

        var map = _session.Map;
        var margin = 24;
        var availableWidth = ClientSize.Width - (margin * 2);
        var availableHeight = ClientSize.Height - (margin * 2);
        var cellSize = Math.Max(8, Math.Min(availableWidth / map.Width, availableHeight / map.Height));
        var gridWidth = cellSize * map.Width;
        var gridHeight = cellSize * map.Height;
        var startX = (ClientSize.Width - gridWidth) / 2;
        var startY = (ClientSize.Height - gridHeight) / 2;

        DrawTerrain(e.Graphics, map, startX, startY, cellSize);
        DrawEntities(e.Graphics, _session.World, startX, startY, cellSize);
    }

    private static void DrawTerrain(Graphics graphics, SandboxMap map, int startX, int startY, int cellSize)
    {
        using var gridPen = new Pen(Color.FromArgb(28, 255, 255, 255), 1);

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var rect = new Rectangle(startX + (x * cellSize), startY + (y * cellSize), cellSize - 1, cellSize - 1);
                using var brush = new SolidBrush(GetTerrainColor(map.GetTerrain(x, y)));
                graphics.FillRectangle(brush, rect);
                graphics.DrawRectangle(gridPen, rect);
            }
        }
    }

    private void DrawEntities(Graphics graphics, WorldState world, int startX, int startY, int cellSize)
    {
        foreach (var entity in world.EntitiesWith<Position2DComponent, DisplayGlyphComponent>())
        {
            var position = entity.GetComponent<Position2DComponent>();
            if (position.X < 0 || position.Y < 0 || _session is null ||
                position.X >= _session.Map.Width || position.Y >= _session.Map.Height)
            {
                continue;
            }

            var icon = ResolveIcon(entity);
            var isSelected = _selectedEntityId == entity.Id;
            var rect = new Rectangle(
                startX + (position.X * cellSize) + 2,
                startY + (position.Y * cellSize) + 2,
                Math.Max(16, cellSize - 5),
                Math.Max(16, cellSize - 5));

            using var shadowBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0));
            using var badgeBrush = new SolidBrush(isSelected ? Color.FromArgb(255, 238, 130) : Color.FromArgb(238, 244, 247));
            using var selectedPen = new Pen(Color.FromArgb(255, 238, 130), 3);
            using var textBrush = new SolidBrush(Color.FromArgb(24, 35, 42));

            graphics.FillEllipse(shadowBrush, rect.X + 2, rect.Y + 3, rect.Width, rect.Height);
            graphics.FillEllipse(badgeBrush, rect);
            if (isSelected)
            {
                graphics.DrawEllipse(selectedPen, rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6);
            }

            var font = icon.Length > 1 ? _emojiFont : _entityFont;
            var size = graphics.MeasureString(icon, font);
            graphics.DrawString(
                icon,
                font,
                textBrush,
                rect.X + ((rect.Width - size.Width) / 2),
                rect.Y + ((rect.Height - size.Height) / 2));
        }
    }

    private static string ResolveIcon(Entity entity)
    {
        if (entity.TryGetComponent<BodyComponent>(out var body) && body is not null)
        {
            var bodyId = body.Body.Id.Value;
            if (bodyId.Contains("human", StringComparison.OrdinalIgnoreCase))
            {
                return "🧍";
            }

            if (bodyId.Contains("deer", StringComparison.OrdinalIgnoreCase))
            {
                return "🦌";
            }

            if (bodyId.Contains("tree", StringComparison.OrdinalIgnoreCase))
            {
                return "🌳";
            }
        }

        if (entity.TryGetComponent<PerceptionSignatureComponent>(out var signature) && signature is not null)
        {
            if (signature.Tags.Contains("water"))
            {
                return "💧";
            }

            if (signature.Tags.Contains("food") || signature.Tags.Contains("fruit"))
            {
                return "🍒";
            }
        }

        if (entity.HasComponent<WaterSourceComponent>())
        {
            return "💧";
        }

        if (entity.HasComponent<ConsumableResourceComponent>())
        {
            return "🍒";
        }

        var glyph = entity.TryGetComponent<DisplayGlyphComponent>(out var display) && display is not null
            ? display.Glyph
            : "?";
        return string.IsNullOrWhiteSpace(glyph) ? "?" : glyph.Trim()[0].ToString();
    }

    private static Color GetTerrainColor(TerrainKind terrain)
    {
        return terrain switch
        {
            TerrainKind.Prairie => Color.FromArgb(92, 142, 83),
            TerrainKind.Forest => Color.FromArgb(39, 91, 63),
            TerrainKind.Water => Color.FromArgb(39, 111, 158),
            TerrainKind.Mountain => Color.FromArgb(120, 124, 128),
            _ => Color.FromArgb(50, 54, 58)
        };
    }

    private void DrawCenteredText(Graphics graphics, string text)
    {
        using var brush = new SolidBrush(Color.FromArgb(190, 205, 214));
        var size = graphics.MeasureString(text, Font);
        graphics.DrawString(text, Font, brush, (ClientSize.Width - size.Width) / 2, (ClientSize.Height - size.Height) / 2);
    }
}
