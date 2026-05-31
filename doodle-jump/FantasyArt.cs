using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DoodleJump;

public static class FantasyArt
{
    public static Color GetClearColor(int scoreMeters) =>
        BiomeArt.ClearColor(BiomeAt.FromMeters(scoreMeters));

    public static void DrawBackground(SpriteBatch batch, Texture2D pixel, int w, int h, int scoreMeters, float animTime)
    {
        switch (BiomeAt.FromMeters(scoreMeters))
        {
            case WorldBiome.CloudKingdom:
                BiomeArt.DrawCloudBackground(batch, pixel, w, h, scoreMeters, animTime);
                return;
            case WorldBiome.Space:
                BiomeArt.DrawSpaceBackground(batch, pixel, w, h, scoreMeters, animTime);
                return;
            default:
                DrawForestBackground(batch, pixel, w, h, scoreMeters, animTime);
                return;
        }
    }

    private static void DrawForestBackground(SpriteBatch batch, Texture2D pixel, int w, int h, int scoreMeters, float animTime)
    {
        var scroll = scoreMeters * 0.15f;

        DrawRect(batch, pixel, new Rectangle(0, 0, w, h), new Color(18, 14, 38));
        DrawGradientBand(batch, pixel, 0, 0, w, h / 2, new Color(48, 32, 88), new Color(28, 22, 58));
        DrawGradientBand(batch, pixel, 0, h / 3, w, h * 2 / 3,
            Rgba(32, 52, 72, 90), Rgba(20, 38, 48, 140));

        DrawAurora(batch, pixel, w, h, animTime);
        DrawMoon(batch, pixel, w, animTime);
        DrawStars(batch, pixel, w, h, scoreMeters);
        DrawParallaxMountains(batch, pixel, w, h, scroll * 0.3f, new Color(38, 34, 62), 0.55f);
        DrawParallaxMountains(batch, pixel, w, h, scroll * 0.55f, new Color(48, 42, 72), 0.42f);
        DrawDistantCastle(batch, pixel, w, h, scroll * 0.2f);
        DrawForestSilhouette(batch, pixel, w, h, scroll * 0.7f);
        DrawFireflies(batch, pixel, w, h, animTime);
        DrawClouds(batch, pixel, w, h, animTime, scroll * 0.1f);
    }

    public static void DrawAmbientDecorations(
        SpriteBatch batch, Texture2D pixel, int w, int h, float animTime, int scoreMeters)
    {
        switch (BiomeAt.FromMeters(scoreMeters))
        {
            case WorldBiome.CloudKingdom:
                BiomeArt.DrawCloudDecorations(batch, pixel, w, h, animTime, scoreMeters);
                return;
            case WorldBiome.Space:
                BiomeArt.DrawSpaceDecorations(batch, pixel, w, h, animTime, scoreMeters);
                return;
        }

        DrawForestDecorations(batch, pixel, w, h, animTime, scoreMeters);
    }

    private static void DrawForestDecorations(
        SpriteBatch batch, Texture2D pixel, int w, int h, float animTime, int scoreMeters)
    {
        var sideScroll = scoreMeters / 4;
        for (var i = 0; i < 5; i++)
        {
            var x = (i * 113 + sideScroll) % (w + 40) - 20;
            var baseY = h - 120 - i * 28;
            DrawHangingVine(batch, pixel, x, baseY, 48 + i * 8, animTime + i);
        }

        for (var i = 0; i < 8; i++)
        {
            var seed = i * 7919 + scoreMeters / 12;
            var fx = Pseudo(seed) % w;
            var fy = 80 + Pseudo(seed + 1) % (h - 200);
            var pulse = 0.5f + 0.5f * MathF.Sin(animTime * 3f + i);
            var size = 2 + (int)(pulse * 2f);
            DrawRect(batch, pixel, new Rectangle(fx, fy, size, size),
                new Color((byte)180, (byte)220, (byte)255, (byte)(80 + pulse * 100)));
        }
    }

    public static void DrawStonePlatform(
        SpriteBatch batch, Texture2D pixel, Rectangle bounds, PlatformKind kind, float animTime, int scoreMeters)
    {
        var seed = bounds.X * 17 + bounds.Y * 31;
        var biome = BiomeAt.FromMeters(scoreMeters);
        if (biome == WorldBiome.CloudKingdom)
        {
            BiomeArt.DrawCloudPlatform(batch, pixel, bounds, kind, animTime, seed);
            return;
        }

        if (biome == WorldBiome.Space)
        {
            BiomeArt.DrawSpacePlatform(batch, pixel, bounds, kind, animTime, seed);
            return;
        }

        DrawForestPlatform(batch, pixel, bounds, kind, animTime, seed);
    }

    private static void DrawForestPlatform(
        SpriteBatch batch, Texture2D pixel, Rectangle bounds, PlatformKind kind, float animTime, int seed)
    {
        var stone = kind switch
        {
            PlatformKind.Moving => new Color(98, 102, 128),
            PlatformKind.Breakable => new Color(118, 82, 68),
            _ => new Color(82, 88, 98)
        };
        var moss = kind switch
        {
            PlatformKind.Breakable => new Color(130, 95, 65),
            _ => new Color(52, 108, 58)
        };
        var dark = Darken(stone, 0.75f);
        var light = Lighten(stone, 1.12f);

        DrawRect(batch, pixel, bounds, dark);
        DrawRect(batch, pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height - 3), stone);

        for (var bx = bounds.X + 4; bx < bounds.Right - 4; bx += 14)
        {
            DrawRect(batch, pixel, new Rectangle(bx, bounds.Y + 5, 10, 4), light);
            DrawRect(batch, pixel, new Rectangle(bx + 2, bounds.Y + 8, 6, 3), dark);
        }

        DrawRect(batch, pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 6), moss);
        DrawGrassTuft(batch, pixel, bounds.X + 6, bounds.Y - 2, 8);
        DrawGrassTuft(batch, pixel, bounds.Right - 14, bounds.Y - 1, 6);

        if (kind == PlatformKind.Breakable)
            DrawCracksPublic(batch, pixel, bounds, seed);

        if (kind == PlatformKind.Moving)
        {
            var glow = (byte)(90 + 50 * MathF.Sin(animTime * 5f + seed));
            DrawRect(batch, pixel, new Rectangle(bounds.X + 4, bounds.Y + 7, bounds.Width - 8, 3),
                new Color((byte)120, (byte)180, (byte)255, glow));
            DrawRune(batch, pixel, bounds.Center.X - 4, bounds.Y + 8, animTime);
        }

        if (Pseudo(seed) % 3 == 0)
            DrawTinyCrystal(batch, pixel, bounds.Right - 10, bounds.Y - 8, animTime + seed);
    }

    public static void DrawFantasyGround(
        SpriteBatch batch, Texture2D pixel, Rectangle bounds, float animTime, int scoreMeters)
    {
        switch (BiomeAt.FromMeters(scoreMeters))
        {
            case WorldBiome.CloudKingdom:
                BiomeArt.DrawCloudGround(batch, pixel, bounds, animTime);
                return;
            case WorldBiome.Space:
                BiomeArt.DrawSpaceGround(batch, pixel, bounds, animTime);
                return;
        }

        DrawForestGround(batch, pixel, bounds, animTime);
    }

    private static void DrawForestGround(SpriteBatch batch, Texture2D pixel, Rectangle bounds, float animTime)
    {
        DrawRect(batch, pixel, bounds, new Color(42, 58, 36));
        DrawRect(batch, pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 18), new Color(58, 102, 52));
        DrawRect(batch, pixel, new Rectangle(bounds.X, bounds.Y + 18, bounds.Width, bounds.Height - 18), new Color(78, 54, 36));

        for (var x = 0; x < bounds.Width; x += 8)
        {
            var blade = 4 + Pseudo(x) % 6;
            var shade = Pseudo(x + 3) % 2 == 0 ? new Color(68, 118, 58) : new Color(48, 92, 48);
            DrawRect(batch, pixel, new Rectangle(x, bounds.Y - blade + 2, 3, blade), shade);
        }

        for (var x = 14; x < bounds.Width; x += 42)
        {
            if (Pseudo(x) % 4 == 0)
                DrawFlower(batch, pixel, x, bounds.Y - 2, Pseudo(x) % 3);
            else
                DrawRect(batch, pixel, new Rectangle(x, bounds.Y + 24, 14, 6), new Color(62, 44, 30));
        }

        DrawRect(batch, pixel, new Rectangle(0, bounds.Y + 40, bounds.Width, 3), new Color(55, 38, 28));
    }

    public static void DrawGoldCoin(
        SpriteBatch batch, Texture2D pixel, Rectangle bounds, float animTime, int scoreMeters)
    {
        var biome = BiomeAt.FromMeters(scoreMeters);
        if (biome == WorldBiome.CloudKingdom)
        {
            BiomeArt.DrawCloudGem(batch, pixel, bounds, animTime);
            return;
        }

        if (biome == WorldBiome.Space)
        {
            BiomeArt.DrawSpaceGem(batch, pixel, bounds, animTime);
            return;
        }

        DrawForestGem(batch, pixel, bounds, animTime);
    }

    private static void DrawForestGem(SpriteBatch batch, Texture2D pixel, Rectangle bounds, float animTime)
    {
        var sparkle = MathF.Sin(animTime * 8f + bounds.X) * 0.5f + 0.5f;
        var outer = new Color(200, 150, 28);
        var inner = new Color(255, 225, 100);
        var gem = new Color(160, 60, 220);

        DrawRect(batch, pixel, Inflate(bounds, 2), new Color((byte)255, (byte)220, (byte)80, (byte)(40 + sparkle * 60)));
        DrawRect(batch, pixel, bounds, outer);
        DrawRect(batch, pixel, Shrink(bounds, 3), inner);
        DrawRect(batch, pixel, new Rectangle(bounds.X + 3, bounds.Y + 3, bounds.Width - 6, 4), Lighten(inner, 1.08f));
        DrawRect(batch, pixel, new Rectangle(bounds.Center.X - 4, bounds.Center.Y - 5, 8, 10), gem);
        DrawRect(batch, pixel, new Rectangle(bounds.Center.X - 2, bounds.Center.Y - 7, 4, 3),
            new Color((byte)255, (byte)255, (byte)255, (byte)(180 + sparkle * 75)));

        if (sparkle > 0.85f)
            DrawRect(batch, pixel, new Rectangle(bounds.Right - 4, bounds.Y + 2, 2, 2), Color.White);
    }

    public static void DrawDragon(SpriteBatch batch, Texture2D pixel, Dragon dragon, float animTime)
    {
        var b = dragon.Bounds;
        var right = dragon.FacingRight;
        var wingFlap = MathF.Sin(animTime * 10f) * 4f;

        var body = new Color(168, 48, 40);
        var belly = new Color(220, 120, 80);
        var wing = new Color(120, 32, 36);
        var horn = new Color(80, 28, 30);

        if (right)
        {
            DrawRect(batch, pixel, new Rectangle(b.X + 4, b.Y + 10, 12, 14 + (int)wingFlap), wing);
            DrawRect(batch, pixel, new Rectangle(b.X + 6, b.Y + 8, 8, 8), Darken(wing, 0.8f));
        }
        else
        {
            DrawRect(batch, pixel, new Rectangle(b.Right - 16, b.Y + 10, 12, 14 + (int)wingFlap), wing);
            DrawRect(batch, pixel, new Rectangle(b.Right - 14, b.Y + 8, 8, 8), Darken(wing, 0.8f));
        }

        DrawRect(batch, pixel, new Rectangle(b.X + 8, b.Y + 16, b.Width - 16, 16), body);
        DrawRect(batch, pixel, new Rectangle(b.X + 12, b.Y + 20, b.Width - 24, 10), belly);

        for (var i = 0; i < 4; i++)
            DrawRect(batch, pixel, new Rectangle(b.X + 12 + i * 8, b.Y + 18, 4, 3), Darken(body, 0.85f));

        DrawRect(batch, pixel, new Rectangle(b.X + (right ? 2 : b.Width - 16), b.Y + 22, 14, 8), Darken(body, 0.7f));
        DrawRect(batch, pixel, new Rectangle(b.X + (right ? b.Width - 18 : 4), b.Y + 24, 14, 6), Darken(body, 0.7f));

        var headX = right ? b.X + 24 : b.X + 6;
        DrawRect(batch, pixel, new Rectangle(headX, b.Y + 4, 22, 16), body);
        DrawRect(batch, pixel, new Rectangle(headX + 4, b.Y + 10, 14, 8), belly);
        DrawRect(batch, pixel, new Rectangle(headX + (right ? 14 : 2), b.Y + 2, 6, 8), horn);
        DrawRect(batch, pixel, new Rectangle(headX + (right ? 16 : 0), b.Y, 4, 6), horn);

        var eyeX = right ? headX + 12 : headX + 4;
        DrawRect(batch, pixel, new Rectangle(eyeX, b.Y + 8, 6, 5), new Color(30, 15, 25));
        DrawRect(batch, pixel, new Rectangle(eyeX + (right ? 3 : 1), b.Y + 9, 2, 2), new Color(255, 240, 80));

        var mouthX = right ? b.Right - 10 : b.X + 4;
        DrawRect(batch, pixel, new Rectangle(mouthX, b.Y + 14, 6, 4), new Color(50, 20, 25));
        DrawRect(batch, pixel, new Rectangle(mouthX + (right ? 4 : 0), b.Y + 13, 3, 2), new Color(255, 160, 60));

        var tailX = right ? b.X : b.Right - 10;
        DrawRect(batch, pixel, new Rectangle(tailX, b.Y + 24, 10, 6), body);
        DrawRect(batch, pixel, new Rectangle(tailX + (right ? -6 : 6), b.Y + 26, 8, 4), Darken(body, 0.8f));
    }

    public static void DrawFireball(SpriteBatch batch, Texture2D pixel, Fireball fb, float animTime)
    {
        var b = fb.Bounds;
        var cx = b.Center.X;
        var cy = b.Center.Y;
        var flicker = MathF.Sin(animTime * 20f + fb.Position.X) * 1.5f;

        DrawRect(batch, pixel, new Rectangle(cx - 7, cy - 7, 14, 14), Rgba(180, 50, 20, 80));
        DrawRect(batch, pixel, new Rectangle(cx - 5, cy - 5, 10, 10), new Color(255, 90, 25));
        DrawRect(batch, pixel, Inflate(b, 1), new Color(255, 140, 40));
        DrawRect(batch, pixel, b, new Color(255, 200, 70));
        DrawRect(batch, pixel, new Rectangle(cx - 2, cy - 3 + (int)flicker, 4, 5), new Color(255, 250, 220));

        DrawRect(batch, pixel, new Rectangle(cx - 8, cy + 2, 3, 2), Rgba(255, 120, 30, 120));
        DrawRect(batch, pixel, new Rectangle(cx + 5, cy + 3, 2, 2), Rgba(255, 100, 20, 100));
    }

    public static void DrawHero(SpriteBatch batch, Texture2D pixel, Vector2 pos, HeroEvolution form, float animTime)
    {
        var squash = 1f + MathF.Sin(animTime * 8f) * 0.06f;
        var wobble = MathF.Sin(animTime * 8f) * 2f;
        var x = (int)pos.X;
        var y = (int)(pos.Y + wobble);
        var extraH = (int)((1f - squash) * 10f);

        DrawEllipseShadow(batch, pixel, x + 6, y + 32 + extraH, 24, 6);

        if (form >= HeroEvolution.Human)
        {
            DrawHuman(batch, pixel, x, y, animTime);
            return;
        }

        DrawSlime(batch, pixel, x, y + extraH, form, animTime, squash);
    }

    private static void DrawSlime(
        SpriteBatch batch, Texture2D pixel, int x, int y, HeroEvolution form, float animTime, float squash)
    {
        var body = new Color(62, 132, 255);
        var shine = new Color(150, 210, 255);
        var shadow = new Color(35, 75, 185);
        var rim = new Color(100, 170, 255);

        var top = y + (int)(8 * squash);
        var midH = (int)(22 * squash);
        var botH = (int)(10 * squash);

        DrawRect(batch, pixel, new Rectangle(x + 5, y + 12 + botH, 26, 8), shadow);
        DrawRect(batch, pixel, new Rectangle(x + 3, top, 30, midH), body);
        DrawRect(batch, pixel, new Rectangle(x + 2, top + 2, 6, midH - 2), rim);
        DrawRect(batch, pixel, new Rectangle(x + 28, top + 4, 4, midH - 6), Darken(body, 0.88f));
        DrawRect(batch, pixel, new Rectangle(x + 8, top + 4, 12, 10), shine);
        DrawRect(batch, pixel, new Rectangle(x + 10, y + 14 + botH, 16, botH), body);
        DrawRect(batch, pixel, new Rectangle(x + 8, y + 18 + botH, 20, 4), shadow);

        var dripPhase = animTime * 6f;
        if (MathF.Sin(dripPhase) > 0.6f)
            DrawRect(batch, pixel, new Rectangle(x + 26, y + 22 + botH, 3, 5), body);

        var eyeY = top + 8;
        DrawRect(batch, pixel, new Rectangle(x + 10, eyeY, 7, 9), Color.White);
        DrawRect(batch, pixel, new Rectangle(x + 22, eyeY, 7, 9), Color.White);
        DrawRect(batch, pixel, new Rectangle(x + 12, eyeY + 2, 4, 6), new Color(18, 28, 90));
        DrawRect(batch, pixel, new Rectangle(x + 24, eyeY + 2, 4, 6), new Color(18, 28, 90));
        DrawRect(batch, pixel, new Rectangle(x + 13, eyeY + 1, 2, 2), Color.White);
        DrawRect(batch, pixel, new Rectangle(x + 25, eyeY + 1, 2, 2), Color.White);
        DrawRect(batch, pixel, new Rectangle(x + 14, eyeY + 10, 4, 2), new Color(40, 80, 160));
        DrawRect(batch, pixel, new Rectangle(x + 24, eyeY + 10, 4, 2), new Color(40, 80, 160));
        DrawRect(batch, pixel, new Rectangle(x + 11, eyeY + 7, 3, 2), Rgba(90, 150, 230, 160));
        DrawRect(batch, pixel, new Rectangle(x + 25, eyeY + 8, 3, 2), Rgba(90, 150, 230, 160));

        if (form >= HeroEvolution.Helmet)
            DrawHelmet(batch, pixel, x, y, animTime);

        if (form >= HeroEvolution.Sword)
            DrawSword(batch, pixel, x, y, animTime);
    }

    private static void DrawHelmet(SpriteBatch batch, Texture2D pixel, int x, int y, float animTime)
    {
        var glint = MathF.Sin(animTime * 4f) > 0.7f;
        DrawRect(batch, pixel, new Rectangle(x + 5, y + 3, 26, 14), new Color(140, 145, 158));
        DrawRect(batch, pixel, new Rectangle(x + 7, y + 5, 22, 10), new Color(198, 202, 215));
        DrawRect(batch, pixel, new Rectangle(x + 13, y + 1, 10, 7), new Color(168, 172, 185));
        DrawRect(batch, pixel, new Rectangle(x + 9, y + 7, 18, 3), new Color(120, 125, 140));
        if (glint)
            DrawRect(batch, pixel, new Rectangle(x + 20, y + 6, 3, 2), Rgba(255, 255, 255, 200));
    }

    private static void DrawSword(SpriteBatch batch, Texture2D pixel, int x, int y, float animTime)
    {
        var sway = (int)(MathF.Sin(animTime * 5f) * 2f);
        DrawRect(batch, pixel, new Rectangle(x + 31 + sway, y + 8, 3, 20), new Color(175, 182, 198));
        DrawRect(batch, pixel, new Rectangle(x + 32 + sway, y + 4, 2, 6), new Color(235, 242, 255));
        DrawRect(batch, pixel, new Rectangle(x + 28 + sway, y + 26, 9, 4), new Color(110, 65, 28));
        DrawRect(batch, pixel, new Rectangle(x + 30 + sway, y + 28, 5, 2), new Color(160, 110, 50));
        DrawRect(batch, pixel, new Rectangle(x + 31 + sway, y + 2, 2, 3), Rgba(255, 240, 180, 180));
    }

    private static void DrawHuman(SpriteBatch batch, Texture2D pixel, int x, int y, float animTime)
    {
        var skin = new Color(255, 205, 170);
        var hair = new Color(78, 48, 30);
        var shirt = new Color(52, 82, 150);
        var pants = new Color(48, 42, 68);
        var cape = new Color(120, 35, 45);
        var boot = new Color(62, 42, 32);

        DrawRect(batch, pixel, new Rectangle(x + 4, y + 18, 8, 22), cape);
        DrawRect(batch, pixel, new Rectangle(x + 26, y + 20, 6, 18), Darken(cape, 0.85f));
        DrawRect(batch, pixel, new Rectangle(x + 8, y + 4, 20, 11), hair);
        DrawRect(batch, pixel, new Rectangle(x + 7, y + 12, 22, 15), skin);
        DrawRect(batch, pixel, new Rectangle(x + 10, y + 18, 5, 4), new Color(35, 28, 28));
        DrawRect(batch, pixel, new Rectangle(x + 21, y + 18, 5, 4), new Color(35, 28, 28));
        DrawRect(batch, pixel, new Rectangle(x + 5, y + 26, 26, 15), shirt);
        DrawRect(batch, pixel, new Rectangle(x + 14, y + 28, 8, 10), Lighten(shirt, 1.1f));
        DrawRect(batch, pixel, new Rectangle(x + 13, y + 34, 10, 3), new Color(180, 150, 60));
        DrawRect(batch, pixel, new Rectangle(x + 7, y + 38, 10, 12), pants);
        DrawRect(batch, pixel, new Rectangle(x + 19, y + 38, 10, 12), pants);
        DrawRect(batch, pixel, new Rectangle(x + 6, y + 48, 12, 5), boot);
        DrawRect(batch, pixel, new Rectangle(x + 18, y + 48, 12, 5), boot);
        DrawHelmet(batch, pixel, x, y, animTime);
        DrawSword(batch, pixel, x, y, animTime);
    }

    #region Decorations

    private static void DrawGradientBand(
        SpriteBatch batch, Texture2D pixel, int x, int y, int w, int h, Color top, Color bottom)
    {
        var steps = Math.Max(4, h / 24);
        for (var i = 0; i < steps; i++)
        {
            var t = i / (float)steps;
            var c = Color.Lerp(top, bottom, t);
            var sh = h / steps + 1;
            DrawRect(batch, pixel, new Rectangle(x, y + i * sh, w, sh), c);
        }
    }

    private static void DrawAurora(SpriteBatch batch, Texture2D pixel, int w, int h, float t)
    {
        for (var band = 0; band < 3; band++)
        {
            var y = 60 + band * 28 + (int)(MathF.Sin(t * 0.6f + band) * 8f);
            var alpha = (byte)(35 + band * 15);
            DrawRect(batch, pixel, new Rectangle(0, y, w, 8), new Color((byte)80, (byte)220, (byte)180, alpha));
            DrawRect(batch, pixel, new Rectangle(0, y + 6, w, 4), new Color((byte)120, (byte)80, (byte)200, (byte)(alpha / 2)));
        }
    }

    private static void DrawMoon(SpriteBatch batch, Texture2D pixel, int w, float t)
    {
        var mx = w - 90 + (int)(MathF.Sin(t * 0.15f) * 6f);
        var my = 48;
        DrawRect(batch, pixel, new Rectangle(mx, my, 36, 36), new Color(240, 235, 210));
        DrawRect(batch, pixel, new Rectangle(mx + 8, my + 6, 28, 30), new Color(220, 215, 195));
        DrawRect(batch, pixel, new Rectangle(mx + 4, my + 10, 8, 8), Rgba(200, 195, 175, 120));
    }

    private static void DrawStars(SpriteBatch batch, Texture2D pixel, int w, int h, int scroll)
    {
        for (var i = 0; i < 40; i++)
        {
            var sx = (i * 53 + scroll / 3) % w;
            var sy = 20 + (i * 29) % (h / 2);
            var size = i % 5 == 0 ? 3 : 2;
            var bright = (byte)(140 + (i % 4) * 30);
            DrawRect(batch, pixel, new Rectangle(sx, sy, size, size), new Color(bright, bright, (byte)220, bright));
        }
    }

    private static void DrawParallaxMountains(
        SpriteBatch batch, Texture2D pixel, int w, int h, float scroll, Color color, float heightFactor)
    {
        var baseY = (int)(h * heightFactor);
        for (var peak = -1; peak <= 3; peak++)
        {
            var cx = peak * 160 - (int)(scroll % 160);
            DrawMountainPeak(batch, pixel, cx, baseY, 120, 70, color);
            DrawMountainPeak(batch, pixel, cx + w / 2, baseY + 15, 90, 50, Darken(color, 0.9f));
        }
    }

    private static void DrawMountainPeak(
        SpriteBatch batch, Texture2D pixel, int cx, int baseY, int width, int height, Color color)
    {
        for (var row = 0; row < height; row++)
        {
            var rowW = (int)(width * (1f - row / (float)height));
            DrawRect(batch, pixel, new Rectangle(cx - rowW / 2, baseY - row, rowW, 1), color);
        }

        DrawRect(batch, pixel, new Rectangle(cx - 8, baseY - height, 16, 8), Lighten(color, 1.15f));
    }

    private static void DrawDistantCastle(SpriteBatch batch, Texture2D pixel, int w, int h, float scroll)
    {
        var bx = (int)(w * 0.22f - scroll % 40f);
        var by = (int)(h * 0.48f);
        var c = new Color(55, 48, 78);
        DrawRect(batch, pixel, new Rectangle(bx, by - 50, 12, 50), c);
        DrawRect(batch, pixel, new Rectangle(bx + 28, by - 38, 10, 38), c);
        DrawRect(batch, pixel, new Rectangle(bx + 52, by - 58, 14, 58), c);
        DrawRect(batch, pixel, new Rectangle(bx - 8, by - 28, 90, 10), Darken(c, 0.9f));
        DrawRect(batch, pixel, new Rectangle(bx + 10, by - 62, 8, 14), Lighten(c, 1.1f));
        DrawRect(batch, pixel, new Rectangle(bx + 54, by - 70, 10, 16), Lighten(c, 1.1f));
        DrawRect(batch, pixel, new Rectangle(bx + 20, by - 20, 6, 8), Rgba(255, 200, 90, 100));
    }

    private static void DrawForestSilhouette(SpriteBatch batch, Texture2D pixel, int w, int h, float scroll)
    {
        var baseY = (int)(h * 0.72f);
        for (var i = -1; i < w / 28 + 2; i++)
        {
            var tx = i * 28 - (int)(scroll % 28);
            DrawTreeSilhouette(batch, pixel, tx, baseY, 22, 38, new Color(24, 48, 32));
        }
    }

    private static void DrawTreeSilhouette(
        SpriteBatch batch, Texture2D pixel, int x, int baseY, int w, int h, Color color)
    {
        DrawRect(batch, pixel, new Rectangle(x + w / 2 - 3, baseY - h / 3, 6, h / 3), Darken(color, 0.7f));
        for (var row = 0; row < h * 2 / 3; row++)
        {
            var rw = w - row / 2;
            DrawRect(batch, pixel, new Rectangle(x + w / 2 - rw / 2, baseY - h + row, rw, 1), color);
        }
    }

    private static void DrawFireflies(SpriteBatch batch, Texture2D pixel, int w, int h, float t)
    {
        for (var i = 0; i < 12; i++)
        {
            var fx = (int)((i * 67f + MathF.Sin(t + i) * 40f) % w);
            var fy = (int)(h * 0.35f + MathF.Sin(t * 1.3f + i * 2f) * 80f + i * 17 % 100);
            var a = (byte)(120 + 80 * MathF.Sin(t * 4f + i));
            DrawRect(batch, pixel, new Rectangle(fx, fy, 3, 3), Rgba(255, 255, 120, a));
        }
    }

    private static void DrawClouds(SpriteBatch batch, Texture2D pixel, int w, int h, float t, float scroll)
    {
        for (var i = 0; i < 4; i++)
        {
            var cx = (int)((i * 140 + scroll + MathF.Sin(t * 0.3f + i) * 20f) % (w + 80) - 40);
            var cy = 90 + i * 35;
            DrawRect(batch, pixel, new Rectangle(cx, cy, 48, 14), Rgba(255, 255, 255, 35));
            DrawRect(batch, pixel, new Rectangle(cx + 12, cy - 6, 28, 12), Rgba(255, 255, 255, 28));
        }
    }

    private static void DrawHangingVine(SpriteBatch batch, Texture2D pixel, int x, int y, int length, float phase)
    {
        for (var i = 0; i < length; i += 4)
        {
            var sway = (int)(MathF.Sin(phase + i * 0.2f) * 3f);
            DrawRect(batch, pixel, new Rectangle(x + sway, y + i, 3, 5), new Color(42, 98, 48));
            if (i % 12 == 0)
                DrawRect(batch, pixel, new Rectangle(x + sway + 3, y + i + 2, 4, 4), new Color(58, 120, 52));
        }
    }

    private static void DrawGrassTuft(SpriteBatch batch, Texture2D pixel, int x, int y, int w)
    {
        DrawRect(batch, pixel, new Rectangle(x, y - 4, 3, 5), new Color(62, 118, 55));
        DrawRect(batch, pixel, new Rectangle(x + w / 2, y - 5, 3, 6), new Color(48, 102, 48));
        DrawRect(batch, pixel, new Rectangle(x + w - 2, y - 3, 3, 4), new Color(72, 128, 60));
    }

    public static void DrawCracksPublic(SpriteBatch batch, Texture2D pixel, Rectangle bounds, int seed) =>
        DrawCracks(batch, pixel, bounds, seed);

    private static void DrawCracks(SpriteBatch batch, Texture2D pixel, Rectangle bounds, int seed)
    {
        var cx = bounds.X + 10 + Pseudo(seed) % (bounds.Width - 20);
        DrawRect(batch, pixel, new Rectangle(cx, bounds.Y + 4, 2, bounds.Height - 4), new Color(50, 35, 30));
        DrawRect(batch, pixel, new Rectangle(cx + 8, bounds.Y + 6, 2, bounds.Height - 6), new Color(50, 35, 30));
        DrawRect(batch, pixel, new Rectangle(cx + 4, bounds.Y + 7, 10, 2), new Color(50, 35, 30));
    }

    private static void DrawRune(SpriteBatch batch, Texture2D pixel, int x, int y, float t)
    {
        var glow = (byte)(160 + 60 * MathF.Sin(t * 6f));
        DrawRect(batch, pixel, new Rectangle(x, y, 2, 5), new Color((byte)140, (byte)200, (byte)255, glow));
        DrawRect(batch, pixel, new Rectangle(x - 2, y + 2, 6, 2), new Color((byte)140, (byte)200, (byte)255, glow));
    }

    private static void DrawTinyCrystal(SpriteBatch batch, Texture2D pixel, int x, int y, float t)
    {
        var pulse = (byte)(150 + 50 * MathF.Sin(t * 5f));
        DrawRect(batch, pixel, new Rectangle(x, y, 6, 8), new Color((byte)100, (byte)200, (byte)255, pulse));
        DrawRect(batch, pixel, new Rectangle(x + 2, y + 1, 2, 6), new Color((byte)200, (byte)240, (byte)255, pulse));
    }

    private static void DrawFlower(SpriteBatch batch, Texture2D pixel, int x, int y, int variant)
    {
        var petal = variant switch
        {
            1 => new Color(255, 120, 150),
            2 => new Color(255, 210, 90),
            _ => new Color(180, 120, 255)
        };
        DrawRect(batch, pixel, new Rectangle(x + 3, y - 2, 3, 4), new Color(58, 110, 50));
        DrawRect(batch, pixel, new Rectangle(x, y - 5, 3, 3), petal);
        DrawRect(batch, pixel, new Rectangle(x + 5, y - 5, 3, 3), petal);
        DrawRect(batch, pixel, new Rectangle(x + 2, y - 7, 4, 3), petal);
        DrawRect(batch, pixel, new Rectangle(x + 3, y - 5, 2, 2), new Color(255, 240, 120));
    }

    private static void DrawEllipseShadow(SpriteBatch batch, Texture2D pixel, int x, int y, int w, int h)
    {
        DrawRect(batch, pixel, new Rectangle(x, y, w, h), Rgba(0, 0, 0, 45));
        DrawRect(batch, pixel, new Rectangle(x + 3, y + 1, w - 6, h - 2), Rgba(0, 0, 0, 30));
    }

    #endregion

    #region Helpers

    private static int Pseudo(int seed)
    {
        seed = (seed ^ (seed >> 16)) * 0x45d9f3b;
        seed = (seed ^ (seed >> 16)) * 0x45d9f3b;
        seed ^= seed >> 16;
        return seed < 0 ? -seed : seed;
    }

    private static Color Rgba(int r, int g, int b, int a) =>
        new Color((byte)r, (byte)g, (byte)b, (byte)a);

    private static Color Rgba(int r, int g, int b, byte a) =>
        new Color((byte)r, (byte)g, (byte)b, a);

    private static Color Darken(Color c, float factor) =>
        new((byte)(c.R * factor), (byte)(c.G * factor), (byte)(c.B * factor), c.A);

    private static Color Lighten(Color c, float factor) =>
        new(
            (byte)Math.Min(255, c.R * factor),
            (byte)Math.Min(255, c.G * factor),
            (byte)Math.Min(255, c.B * factor),
            c.A);

    private static Rectangle Inflate(Rectangle r, int amount) =>
        new(r.X - amount, r.Y - amount, r.Width + amount * 2, r.Height + amount * 2);

    private static Rectangle Shrink(Rectangle r, int amount) =>
        new(r.X + amount, r.Y + amount, r.Width - amount * 2, r.Height - amount * 2);

    private static void DrawRect(SpriteBatch batch, Texture2D pixel, Rectangle rect, Color color) =>
        batch.Draw(pixel, rect, color);

    #endregion
}
