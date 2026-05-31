using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DoodleJump;

/// <summary>Cloud kingdom (10k–30k m) and space (30k+ m) visuals.</summary>
public static class BiomeArt
{
    public static Color ClearColor(WorldBiome biome) => biome switch
    {
        WorldBiome.CloudKingdom => new Color(186, 218, 255),
        WorldBiome.Space => new Color(6, 4, 18),
        _ => new Color(42, 32, 72)
    };

    public static void DrawCloudBackground(SpriteBatch batch, Texture2D pixel, int w, int h, int scoreMeters, float animTime)
    {
        var scroll = scoreMeters * 0.12f;
        DrawRect(batch, pixel, new Rectangle(0, 0, w, h), new Color(200, 228, 255));
        DrawGradientBand(batch, pixel, 0, 0, w, h, new Color(255, 248, 220), new Color(170, 210, 255));
        DrawGradientBand(batch, pixel, 0, h / 2, w, h / 2, new Color(255, 255, 255, 60), new Color(140, 190, 255));

        DrawSunRays(batch, pixel, w, h, animTime);
        DrawBigCloud(batch, pixel, w, h, animTime, scroll);
        DrawFloatingIslands(batch, pixel, w, h, scroll);
        DrawHeavenPillars(batch, pixel, w, h, scroll * 0.25f);
        DrawHolySparkles(batch, pixel, w, h, animTime);
    }

    public static void DrawCloudDecorations(SpriteBatch batch, Texture2D pixel, int w, int h, float animTime, int scoreMeters)
    {
        var scroll = scoreMeters / 5;
        for (var i = 0; i < 6; i++)
        {
            var x = (i * 97 + scroll) % (w + 30) - 15;
            var y = 50 + i * 55 + (int)(MathF.Sin(animTime + i) * 12f);
            DrawMiniCloud(batch, pixel, x, y, 36 + i * 4);
        }

        for (var i = 0; i < 5; i++)
        {
            var x = (i * 140 + scroll / 2) % w;
            DrawLightPillar(batch, pixel, x, h - 100 - i * 20, animTime + i);
        }
    }

    public static void DrawCloudPlatform(
        SpriteBatch batch, Texture2D pixel, Rectangle bounds, PlatformKind kind, float animTime, int seed)
    {
        // Контраст с бело-голубым небом: лавандовое тело + тёмная обводка + золотой верх.
        var outline = new Color(72, 88, 168);
        var shadow = new Color(58, 72, 140);
        var body = kind switch
        {
            PlatformKind.Moving => new Color(118, 145, 235),
            PlatformKind.Breakable => new Color(195, 128, 145),
            _ => new Color(102, 128, 220)
        };
        var bodyLight = Lighten(body, 1.14f);
        var bodyDark = Darken(body, 0.82f);
        var goldTop = new Color(255, 196, 48);
        var goldDark = new Color(210, 140, 30);

        DrawRect(batch, pixel, new Rectangle(bounds.X + 2, bounds.Y + 3, bounds.Width, bounds.Height), shadow);
        DrawRect(batch, pixel, Inflate(bounds, 1), outline);
        DrawRect(batch, pixel, bounds, bodyDark);
        DrawRect(batch, pixel, new Rectangle(bounds.X + 2, bounds.Y + 2, bounds.Width - 4, bounds.Height - 3), body);
        DrawRect(batch, pixel, new Rectangle(bounds.X + 3, bounds.Y + 4, bounds.Width - 10, 4), bodyLight);
        DrawRect(batch, pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 5), goldDark);
        DrawRect(batch, pixel, new Rectangle(bounds.X + 1, bounds.Y, bounds.Width - 2, 4), goldTop);

        var pulse = (byte)(70 + 45 * MathF.Sin(animTime * 4f + seed));
        DrawRect(batch, pixel, Inflate(bounds, 2), Rgba(80, 100, 200, pulse / 3));

        if (kind == PlatformKind.Breakable)
            FantasyArt.DrawCracksPublic(batch, pixel, bounds, seed);

        if (kind == PlatformKind.Moving)
        {
            DrawRect(batch, pixel, new Rectangle(bounds.Center.X - 4, bounds.Y + 7, 8, 4), new Color(255, 245, 180));
            DrawRuneAccent(batch, pixel, bounds.Center.X - 2, bounds.Y + 8, animTime);
        }
    }

    private static void DrawRuneAccent(SpriteBatch batch, Texture2D pixel, int x, int y, float t)
    {
        var glow = (byte)(200 + 55 * MathF.Sin(t * 6f));
        DrawRect(batch, pixel, new Rectangle(x, y, 2, 4), new Color((byte)255, (byte)255, (byte)220, glow));
        DrawRect(batch, pixel, new Rectangle(x - 2, y + 1, 6, 2), new Color((byte)255, (byte)240, (byte)160, glow));
    }

    public static void DrawCloudGround(SpriteBatch batch, Texture2D pixel, Rectangle bounds, float animTime)
    {
        DrawRect(batch, pixel, bounds, new Color(220, 235, 255));
        for (var x = 0; x < bounds.Width; x += 24)
            DrawMiniCloud(batch, pixel, x, bounds.Y - 8, 28 + (int)(MathF.Sin(animTime + x) * 4f));

        DrawRect(batch, pixel, new Rectangle(0, bounds.Y + 30, bounds.Width, bounds.Height - 30), new Color(255, 248, 220, 180));
    }

    public static void DrawCloudGem(SpriteBatch batch, Texture2D pixel, Rectangle bounds, float animTime)
    {
        var sparkle = MathF.Sin(animTime * 8f + bounds.X) * 0.5f + 0.5f;
        DrawRect(batch, pixel, Inflate(bounds, 2), Rgba(255, 255, 255, (byte)(50 + sparkle * 80)));
        DrawRect(batch, pixel, bounds, new Color(255, 240, 180));
        DrawRect(batch, pixel, Shrink(bounds, 3), new Color(255, 255, 255));
        DrawRect(batch, pixel, new Rectangle(bounds.Center.X - 3, bounds.Center.Y - 4, 6, 8), new Color(255, 210, 80));
    }

    public static void DrawSpaceBackground(SpriteBatch batch, Texture2D pixel, int w, int h, int scoreMeters, float animTime)
    {
        var scroll = scoreMeters * 0.08f;
        DrawRect(batch, pixel, new Rectangle(0, 0, w, h), new Color(6, 4, 18));
        DrawGradientBand(batch, pixel, 0, 0, w, h / 2, new Color(18, 10, 42), new Color(4, 2, 12));

        DrawNebula(batch, pixel, w, h, animTime);
        DrawSpaceStars(batch, pixel, w, h, scoreMeters);
        DrawPlanet(batch, pixel, w, h, scroll);
        DrawDistantGalaxy(batch, pixel, w, h, animTime);
    }

    public static void DrawSpaceDecorations(SpriteBatch batch, Texture2D pixel, int w, int h, float animTime, int scoreMeters)
    {
        for (var i = 0; i < 6; i++)
        {
            var sx = (i * 83 + scoreMeters / 4) % w;
            var sy = 40 + (i * 61) % (h - 80);
            var blink = MathF.Sin(animTime * 2f + i) > 0.3f;
            if (blink)
                DrawRect(batch, pixel, new Rectangle(sx, sy, 2, 2), new Color(200, 220, 255));
        }

        for (var i = 0; i < 3; i++)
        {
            var x = (i * 160 + (int)(animTime * 20f)) % (w + 60) - 30;
            var y = 120 + i * 90;
            DrawSatellite(batch, pixel, x, y, animTime + i);
        }
    }

    public static void DrawSpacePlatform(
        SpriteBatch batch, Texture2D pixel, Rectangle bounds, PlatformKind kind, float animTime, int seed)
    {
        var rock = kind switch
        {
            PlatformKind.Moving => new Color(90, 88, 110),
            PlatformKind.Breakable => new Color(70, 65, 80),
            _ => new Color(78, 82, 98)
        };
        var crater = Darken(rock, 0.7f);

        DrawRect(batch, pixel, bounds, Darken(rock, 0.55f));
        DrawRect(batch, pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height - 2), rock);

        for (var bx = bounds.X + 6; bx < bounds.Right - 8; bx += 16)
            DrawRect(batch, pixel, new Rectangle(bx, bounds.Y + 4, 5, 3), crater);

        if (kind == PlatformKind.Moving)
        {
            var pulse = (byte)(100 + 80 * MathF.Sin(animTime * 6f + seed));
            DrawRect(batch, pixel, new Rectangle(bounds.X + 2, bounds.Y + 5, bounds.Width - 4, 2),
                new Color((byte)80, (byte)220, (byte)255, pulse));
        }

        if (kind == PlatformKind.Breakable)
            FantasyArt.DrawCracksPublic(batch, pixel, bounds, seed);
    }

    public static void DrawSpaceGround(SpriteBatch batch, Texture2D pixel, Rectangle bounds, float animTime)
    {
        DrawRect(batch, pixel, bounds, new Color(55, 52, 62));
        DrawRect(batch, pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 20), new Color(120, 118, 128));

        for (var x = 8; x < bounds.Width; x += 22)
        {
            var cr = 4 + (x % 7);
            DrawRect(batch, pixel, new Rectangle(x, bounds.Y + 8, cr * 2, cr), new Color(75, 72, 82));
        }
    }

    public static void DrawSpaceGem(SpriteBatch batch, Texture2D pixel, Rectangle bounds, float animTime)
    {
        var sparkle = MathF.Sin(animTime * 8f + bounds.X) * 0.5f + 0.5f;
        DrawRect(batch, pixel, Inflate(bounds, 2), Rgba(80, 200, 255, (byte)(40 + sparkle * 70)));
        DrawRect(batch, pixel, bounds, new Color(40, 160, 220));
        DrawRect(batch, pixel, Shrink(bounds, 3), new Color(120, 230, 255));
        DrawRect(batch, pixel, new Rectangle(bounds.Center.X - 2, bounds.Center.Y - 5, 4, 8), new Color(200, 255, 255));
    }

    #region Cloud details

    private static void DrawSunRays(SpriteBatch batch, Texture2D pixel, int w, int h, float t)
    {
        var cx = w / 2;
        var cy = 80 + (int)(MathF.Sin(t * 0.4f) * 8f);
        for (var i = 0; i < 8; i++)
        {
            var angle = i * MathF.PI / 4f + t * 0.1f;
            var ex = cx + (int)(MathF.Cos(angle) * 120f);
            var ey = cy + (int)(MathF.Sin(angle) * 50f);
            DrawRect(batch, pixel, new Rectangle(Math.Min(cx, ex), Math.Min(cy, ey), Math.Abs(ex - cx) + 2, 3),
                Rgba(255, 250, 200, 40));
        }

        DrawRect(batch, pixel, new Rectangle(cx - 28, cy - 28, 56, 56), new Color(255, 245, 180, 120));
        DrawRect(batch, pixel, new Rectangle(cx - 18, cy - 18, 36, 36), new Color(255, 255, 220));
    }

    private static void DrawBigCloud(SpriteBatch batch, Texture2D pixel, int w, int h, float t, float scroll)
    {
        for (var layer = 0; layer < 5; layer++)
        {
            var y = 100 + layer * 70 + (int)(MathF.Sin(t * 0.5f + layer) * 10f);
            for (var i = -1; i < 4; i++)
            {
                var cx = i * 130 - (int)(scroll * (0.3f + layer * 0.1f)) % 130;
                DrawMiniCloud(batch, pixel, cx, y, 70 + layer * 8);
                DrawMiniCloud(batch, pixel, cx + 40, y - 12, 50);
            }
        }
    }

    private static void DrawMiniCloud(SpriteBatch batch, Texture2D pixel, int x, int y, int size)
    {
        var c = new Color(255, 255, 255, 200);
        DrawRect(batch, pixel, new Rectangle(x, y + size / 3, size, size / 2), c);
        DrawRect(batch, pixel, new Rectangle(x + size / 5, y, size / 2, size / 2), c);
        DrawRect(batch, pixel, new Rectangle(x + size / 2, y + size / 5, size / 2, size / 2), c);
    }

    private static void DrawFloatingIslands(SpriteBatch batch, Texture2D pixel, int w, int h, float scroll)
    {
        for (var i = 0; i < 3; i++)
        {
            var ix = (int)(w * (0.15f + i * 0.3f) - scroll * 0.15f % 60f);
            var iy = (int)(h * (0.55f + i * 0.08f));
            DrawRect(batch, pixel, new Rectangle(ix, iy, 50, 12), new Color(100, 180, 90));
            DrawRect(batch, pixel, new Rectangle(ix - 6, iy + 10, 62, 18), new Color(180, 160, 130));
        }
    }

    private static void DrawHeavenPillars(SpriteBatch batch, Texture2D pixel, int w, int h, float scroll)
    {
        var px = (int)(w * 0.7f - scroll % 30f);
        DrawRect(batch, pixel, new Rectangle(px, (int)(h * 0.35f), 14, (int)(h * 0.4f)), new Color(255, 240, 200, 140));
        DrawRect(batch, pixel, new Rectangle(px - 4, (int)(h * 0.33f), 22, 12), new Color(255, 230, 150));
        DrawRect(batch, pixel, new Rectangle(px + 2, (int)(h * 0.33f), 8, (int)(h * 0.38f)), new Color(255, 255, 255, 80));
    }

    private static void DrawHolySparkles(SpriteBatch batch, Texture2D pixel, int w, int h, float t)
    {
        for (var i = 0; i < 16; i++)
        {
            var sx = (int)((i * 59f + MathF.Sin(t * 2f + i) * 30f) % w);
            var sy = (int)(80 + MathF.Sin(t * 1.5f + i * 1.7f) * 120f + i * 11 % 200);
            var a = (byte)(100 + 100 * MathF.Sin(t * 5f + i));
            DrawRect(batch, pixel, new Rectangle(sx, sy, 2, 2), Rgba(255, 255, 220, a));
        }
    }

    private static void DrawLightPillar(SpriteBatch batch, Texture2D pixel, int x, int y, float phase)
    {
        var h = 60 + (int)(MathF.Sin(phase) * 10f);
        DrawRect(batch, pixel, new Rectangle(x, y - h, 4, h), Rgba(255, 255, 255, 50));
        DrawRect(batch, pixel, new Rectangle(x + 1, y - h / 2, 2, h / 2), Rgba(255, 240, 180, 80));
    }

    #endregion

    #region Space details

    private static void DrawNebula(SpriteBatch batch, Texture2D pixel, int w, int h, float t)
    {
        var nx = w / 3 + (int)(MathF.Sin(t * 0.2f) * 20f);
        var ny = h / 3;
        DrawRect(batch, pixel, new Rectangle(nx, ny, 140, 80), Rgba(120, 40, 160, 35));
        DrawRect(batch, pixel, new Rectangle(nx + 40, ny + 20, 100, 50), Rgba(40, 80, 200, 40));
        DrawRect(batch, pixel, new Rectangle(w * 2 / 3, h / 2, 90, 60), Rgba(200, 60, 100, 30));
    }

    private static void DrawSpaceStars(SpriteBatch batch, Texture2D pixel, int w, int h, int scroll)
    {
        for (var i = 0; i < 55; i++)
        {
            var sx = (i * 41 + scroll / 2) % w;
            var sy = (i * 29) % h;
            var size = i % 7 == 0 ? 3 : 2;
            var tint = i % 3 == 0 ? new Color(200, 220, 255) : Color.White;
            DrawRect(batch, pixel, new Rectangle(sx, sy, size, size), tint);
        }
    }

    private static void DrawPlanet(SpriteBatch batch, Texture2D pixel, int w, int h, float scroll)
    {
        var px = (int)(w * 0.75f - scroll % 20f);
        var py = (int)(h * 0.2f);
        DrawRect(batch, pixel, new Rectangle(px, py, 48, 48), new Color(180, 90, 60));
        DrawRect(batch, pixel, new Rectangle(px + 8, py + 10, 32, 8), new Color(140, 70, 45));
        DrawRect(batch, pixel, new Rectangle(px - 6, py + 20, 60, 6), new Color(200, 200, 220, 60));
    }

    private static void DrawDistantGalaxy(SpriteBatch batch, Texture2D pixel, int w, int h, float t)
    {
        var gx = w / 5;
        var gy = h / 2 + (int)(MathF.Sin(t * 0.3f) * 15f);
        for (var i = 0; i < 6; i++)
        {
            var angle = i * 1.2f + t * 0.15f;
            var dx = (int)(MathF.Cos(angle) * 22f);
            var dy = (int)(MathF.Sin(angle) * 10f);
            DrawRect(batch, pixel, new Rectangle(gx + dx, gy + dy, 4, 2), new Color(180, 160, 255, 120));
        }
    }

    private static void DrawSatellite(SpriteBatch batch, Texture2D pixel, int x, int y, float t)
    {
        var sway = (int)(MathF.Sin(t) * 4f);
        DrawRect(batch, pixel, new Rectangle(x + sway, y, 14, 6), new Color(180, 185, 200));
        DrawRect(batch, pixel, new Rectangle(x - 8 + sway, y + 1, 8, 4), new Color(120, 180, 255, 150));
        DrawRect(batch, pixel, new Rectangle(x + 14 + sway, y + 1, 8, 4), new Color(120, 180, 255, 150));
    }

    #endregion

    private static void DrawGradientBand(
        SpriteBatch batch, Texture2D pixel, int x, int y, int w, int h, Color top, Color bottom)
    {
        var steps = Math.Max(4, h / 24);
        for (var i = 0; i < steps; i++)
        {
            var t = i / (float)steps;
            var sh = h / steps + 1;
            DrawRect(batch, pixel, new Rectangle(x, y + i * sh, w, sh), Color.Lerp(top, bottom, t));
        }
    }

    private static Color Darken(Color c, float f) =>
        new((byte)(c.R * f), (byte)(c.G * f), (byte)(c.B * f), c.A);

    private static Color Lighten(Color c, float f) =>
        new(
            (byte)Math.Min(255, c.R * f),
            (byte)Math.Min(255, c.G * f),
            (byte)Math.Min(255, c.B * f),
            c.A);

    private static Rectangle Shrink(Rectangle r, int a) =>
        new(r.X + a, r.Y + a, r.Width - a * 2, r.Height - a * 2);

    private static Rectangle Inflate(Rectangle r, int a) =>
        new(r.X - a, r.Y - a, r.Width + a * 2, r.Height + a * 2);

    private static Color Rgba(int r, int g, int b, int a) =>
        new((byte)r, (byte)g, (byte)b, (byte)a);

    private static Color Rgba(int r, int g, int b, byte a) =>
        new((byte)r, (byte)g, (byte)b, a);

    private static void DrawRect(SpriteBatch batch, Texture2D pixel, Rectangle rect, Color color) =>
        batch.Draw(pixel, rect, color);
}
