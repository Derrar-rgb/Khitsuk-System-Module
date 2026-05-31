using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DoodleJump;

public class Game1 : Game
{
    private const int ScreenWidth = 480;
    private const int ScreenHeight = 800;
    private const int CoinsForMegaJump = 10;
    private const string DeathMessage = "YOU DIED";

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private SpriteFont? _font;

    private readonly Player _player = new();
    private readonly Ground _ground = new();
    private readonly List<Platform> _platforms = new();
    private readonly List<Coin> _coins = new();
    private readonly PlatformSpawner _spawner = new();
    private readonly CoinSpawner _coinSpawner = new();
    private readonly DragonSpawner _dragonSpawner = new();
    private readonly List<Dragon> _dragons = new();
    private readonly List<Fireball> _fireballs = new();
    private readonly Random _random = new();
    private float _heroAnimTime;
    private int _dragonKills;

    private KeyboardState _previousKeyboard;
    private int _score;
    private int _highScore;
    private int _coinCount;
    private float _coinAnimTime;
    private float _deathFade;
    private float _superJumpBanner;
    private float _secretKillBanner;
    private bool _isGameOver;

    private int _secretComboStep;
    private float _secretComboResetTimer;
    private static readonly Keys[] SecretDragonCombo = { Keys.D2, Keys.D8, Keys.D8 };

    /// <summary>Пройденная высота вверх: 1 единица счёта = 1 метр.</summary>
    private int MetersClimbed => _score;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = ScreenWidth,
            PreferredBackBufferHeight = ScreenHeight
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        StartNewRun();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        try
        {
            _font = Content.Load<SpriteFont>("Fonts/Main");
        }
        catch
        {
            _font = null;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (keyboard.IsKeyDown(Keys.Escape))
            Exit();

        if (WasPressed(keyboard, Keys.Enter) || WasPressed(keyboard, Keys.Space))
        {
            if (_isGameOver)
                StartNewRun();
        }

        if (!_isGameOver)
            UpdateGameplay(dt, keyboard);
        else
        {
            _deathFade = Math.Min(1f, _deathFade + dt * 0.65f);
            if (WasPressed(keyboard, Keys.R))
                StartNewRun();
        }

        _previousKeyboard = keyboard;
        base.Update(gameTime);
    }

    private void UpdateGameplay(float dt, KeyboardState keyboard)
    {
        _coinAnimTime += dt;
        _heroAnimTime += dt;
        if (_superJumpBanner > 0f)
            _superJumpBanner -= dt;
        if (_secretKillBanner > 0f)
            _secretKillBanner -= dt;

        _player.Update(dt, keyboard, ScreenWidth);

        foreach (var platform in _platforms)
            platform.Update(dt, ScreenWidth);

        foreach (var dragon in _dragons)
            dragon.Update(dt, ScreenWidth, _fireballs, _random);

        UpdateFireballs(dt);

        UpdateSecretCombo(dt, keyboard);

        TryLandOnSurfaces(dt);
        TryCollectCoins();
        UpdateDragonSpawns();

        var scrollLine = ScreenHeight * 0.42f;
        if (_player.Position.Y < scrollLine)
        {
            var delta = scrollLine - _player.Position.Y;
            _player.Position.Y = scrollLine;
            ScrollWorld(delta);
            _score += (int)delta;
        }

        var platformCountBefore = _platforms.Count;
        _spawner.EnsurePlatformsAbove(_platforms, _player.Position.Y - ScreenHeight, ScreenWidth, ScreenHeight);
        for (var i = platformCountBefore; i < _platforms.Count; i++)
            _coinSpawner.TrySpawnOnPlatform(_coins, _platforms[i]);

        _spawner.RemoveBelow(_platforms, _player.Position.Y + ScreenHeight);
        _coinSpawner.RemoveBelow(_coins, _player.Position.Y + ScreenHeight);
        _dragonSpawner.RemoveBelow(_dragons, _player.Position.Y + ScreenHeight);
        _coins.RemoveAll(c => c.Collected);
        _fireballs.RemoveAll(f => !f.Active);
        _dragons.RemoveAll(d => !d.Active);

        if (_player.Position.Y > ScreenHeight + 40f)
            TriggerDeath();
    }

    private void TryCollectCoins()
    {
        var pb = _player.Bounds;
        var playerBounds = new Rectangle(pb.X - 4, pb.Y - 4, pb.Width + 8, pb.Height + 8);

        foreach (var coin in _coins)
        {
            if (coin.Collected)
                continue;

            if (!playerBounds.Intersects(coin.Bounds))
                continue;

            coin.Collect();
            _coinCount++;

            if (_coinCount < CoinsForMegaJump)
                continue;

            TriggerMegaJump();
            break;
        }
    }

    private void TriggerMegaJump()
    {
        _coinCount = 0;
        _player.MegaJump();
        _superJumpBanner = 1.4f;
    }

    private void TriggerDeath()
    {
        _isGameOver = true;
        _deathFade = 0f;
    }

    private void UpdateFireballs(float dt)
    {
        var playerBounds = ExpandedPlayerBounds();

        foreach (var fireball in _fireballs)
        {
            if (!fireball.Active)
                continue;

            fireball.Position += fireball.Velocity * dt;
            fireball.Velocity.Y += 420f * dt;

            if (fireball.Position.Y > ScreenHeight + 60f || fireball.Position.X < -40f ||
                fireball.Position.X > ScreenWidth + 40f)
            {
                fireball.Deactivate();
                continue;
            }

            if (playerBounds.Intersects(fireball.Bounds))
                TriggerDeath();
        }
    }

    private void UpdateDragonSpawns() =>
        _dragonSpawner.TrySpawnAtMilestones(_score, _platforms, _dragons, _player.Position.Y);

    private HeroEvolution CurrentEvolution =>
        (HeroEvolution)Math.Min((int)HeroEvolution.Human, _dragonKills);

    private void OnDragonStomped()
    {
        _dragonKills++;
        _player.Bounce();
    }

    private void UpdateSecretCombo(float dt, KeyboardState keyboard)
    {
        if (_secretComboStep > 0)
        {
            _secretComboResetTimer -= dt;
            if (_secretComboResetTimer <= 0f)
                _secretComboStep = 0;
        }

        if (_secretComboStep >= SecretDragonCombo.Length)
            _secretComboStep = 0;

        var expected = SecretDragonCombo[_secretComboStep];
        if (!WasSecretComboKeyPressed(keyboard, expected))
            return;

        _secretComboStep++;
        _secretComboResetTimer = 2.5f;

        if (_secretComboStep < SecretDragonCombo.Length)
            return;

        _secretComboStep = 0;
        TrySecretDragonAssassin();
    }

    private bool WasSecretComboKeyPressed(KeyboardState current, Keys key) =>
        key switch
        {
            Keys.D2 => WasPressed(current, Keys.D2) || WasPressed(current, Keys.NumPad2),
            Keys.D8 => WasPressed(current, Keys.D8) || WasPressed(current, Keys.NumPad8),
            _ => WasPressed(current, key)
        };

    private void TrySecretDragonAssassin()
    {
        if (_dragonKills >= DragonSpawner.DragonCount)
            return;

        _spawner.EnsurePlatformsAbove(
            _platforms,
            _player.Position.Y - ScreenHeight,
            ScreenWidth,
            ScreenHeight);

        var slot = _dragonKills;
        var dragon = FindLiveDragonForAssassin(slot)
                     ?? _dragonSpawner.CheatSpawnForSlot(slot, _platforms, _dragons, _player.Position.Y);

        if (dragon == null || !dragon.Active)
            return;

        TeleportOntoAndKillDragon(dragon);
        _secretKillBanner = 1.1f;
    }

    private Dragon? FindLiveDragonForAssassin(int slot)
    {
        var alive = _dragons.Where(d => d.Active).OrderBy(d => d.Position.Y).ToList();
        if (alive.Count == 0)
            return null;

        return slot < alive.Count ? alive[slot] : alive[0];
    }

    private void TeleportOntoAndKillDragon(Dragon dragon)
    {
        var surface = dragon.StompSurface;
        _player.Position = new Vector2(
            dragon.Position.X + Dragon.Width / 2f - Player.Width / 2f,
            surface.Top - Player.Height - 2f);
        _player.Velocity = Vector2.Zero;

        dragon.Kill();
        _dragonKills++;
        _player.Bounce();

        foreach (var fireball in _fireballs)
            fireball.Deactivate();
    }

    private Rectangle ExpandedPlayerBounds()
    {
        var pb = _player.Bounds;
        return new Rectangle(pb.X - 4, pb.Y - 4, pb.Width + 8, pb.Height + 8);
    }

    private void TryLandOnSurfaces(float dt)
    {
        if (!_player.IsFalling)
            return;

        var feet = _player.Position.Y + Player.Height;
        var previousFeet = feet - _player.Velocity.Y * dt;

        if (TryStompDragon(previousFeet, feet))
            return;

        if (TryLandOnGround(previousFeet, feet))
            return;

        foreach (var platform in _platforms)
        {
            if (!platform.Active)
                continue;

            if (TryLandOnSurface(previousFeet, feet, platform.Bounds))
            {
                if (platform.Kind == PlatformKind.Breakable)
                    platform.Break();
                return;
            }
        }
    }

    private bool TryLandOnGround(float previousFeet, float feet)
    {
        var top = _ground.TopY;
        if (previousFeet <= top && feet >= top &&
            feet <= top + 12f)
        {
            return TryLandOnSurface(previousFeet, feet, new Rectangle(0, (int)top, _ground.Width, 1));
        }

        return false;
    }

    private bool TryLandOnSurface(float previousFeet, float feet, Rectangle bounds)
    {
        if (previousFeet > bounds.Top || feet < bounds.Top ||
            feet > bounds.Top + Platform.Height + 10f ||
            _player.Position.X + Player.Width <= bounds.Left + 4f ||
            _player.Position.X >= bounds.Right - 4f)
        {
            return false;
        }

        _player.Position.Y = bounds.Top - Player.Height;
        _player.Bounce();
        return true;
    }

    private bool TryStompDragon(float previousFeet, float feet)
    {
        foreach (var dragon in _dragons)
        {
            if (!dragon.Active)
                continue;

            var surface = dragon.StompSurface;
            if (previousFeet > surface.Top || feet < surface.Top ||
                feet > surface.Top + 14f ||
                _player.Position.X + Player.Width <= surface.Left + 2f ||
                _player.Position.X >= surface.Right - 2f)
            {
                continue;
            }

            dragon.Kill();
            OnDragonStomped();
            _player.Position.Y = surface.Top - Player.Height;
            return true;
        }

        return false;
    }

    private void ScrollWorld(float delta)
    {
        _player.Position.Y += delta;

        foreach (var platform in _platforms)
            platform.Position.Y += delta;

        _spawner.Scroll(delta);
        _coinSpawner.Scroll(_coins, delta);
        _dragonSpawner.Scroll(_dragons, delta);

        foreach (var fireball in _fireballs)
        {
            if (fireball.Active)
                fireball.Position.Y += delta;
        }

        _ground.Scroll(delta);
    }

    private void StartNewRun()
    {
        _isGameOver = false;
        _score = 0;
        _coinCount = 0;
        _deathFade = 0f;
        _superJumpBanner = 0f;
        _secretKillBanner = 0f;
        _secretComboStep = 0;
        _secretComboResetTimer = 0f;
        _dragonKills = 0;
        _heroAnimTime = 0f;
        _dragons.Clear();
        _fireballs.Clear();
        _dragonSpawner.Reset();

        _ground.Reset(ScreenWidth, ScreenHeight);

        _player.Reset(new Vector2(
            ScreenWidth / 2f - Player.Width / 2f,
            _ground.TopY - Player.Height));

        _spawner.Reset(_ground.TopY - 55f, 14, ScreenWidth, ScreenHeight, _platforms);
        _coinSpawner.Reset(_coins, _platforms, ScreenWidth);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(FantasyArt.GetClearColor(_score));

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        FantasyArt.DrawBackground(_spriteBatch, _pixel, ScreenWidth, ScreenHeight, _score, _heroAnimTime);
        FantasyArt.DrawAmbientDecorations(_spriteBatch, _pixel, ScreenWidth, ScreenHeight, _heroAnimTime, _score);
        DrawPlatforms();
        DrawGround();
        DrawCoins();
        DrawDragons();
        DrawFireballs();
        DrawPlayer();
        DrawHud();

        if (_superJumpBanner > 0f)
            DrawSuperJumpBanner();

        if (_secretKillBanner > 0f)
            DrawSecretKillBanner();

        if (_isGameOver)
            DrawDarkSoulsDeath();

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawGround() =>
        FantasyArt.DrawFantasyGround(_spriteBatch, _pixel, _ground.Bounds, _heroAnimTime, _score);

    private void DrawPlatforms()
    {
        foreach (var platform in _platforms)
        {
            if (!platform.Active)
                continue;

            FantasyArt.DrawStonePlatform(_spriteBatch, _pixel, platform.Bounds, platform.Kind, _heroAnimTime, _score);
        }
    }

    private void DrawDragons()
    {
        foreach (var dragon in _dragons)
        {
            if (dragon.Active)
                FantasyArt.DrawDragon(_spriteBatch, _pixel, dragon, _heroAnimTime);
        }
    }

    private void DrawFireballs()
    {
        foreach (var fireball in _fireballs)
        {
            if (fireball.Active)
                FantasyArt.DrawFireball(_spriteBatch, _pixel, fireball, _heroAnimTime);
        }
    }

    private void DrawPlayer() =>
        FantasyArt.DrawHero(_spriteBatch, _pixel, _player.Position, CurrentEvolution, _heroAnimTime);

    private void DrawCoins()
    {
        foreach (var coin in _coins)
        {
            if (coin.Collected)
                continue;

            var bob = MathF.Sin(_coinAnimTime * 6f + coin.SpinPhase) * 3f;
            var bounds = coin.Bounds;
            bounds.Offset(0, (int)bob);

            FantasyArt.DrawGoldCoin(_spriteBatch, _pixel, bounds, _coinAnimTime, _score);
        }
    }

    private void DrawHud()
    {
        DrawMetersCounter();

        var coinText = $"Gems: {_coinCount}/{CoinsForMegaJump}";
        var formText = CurrentEvolution switch
        {
            HeroEvolution.Human => "Hero: Human",
            HeroEvolution.Sword => "Hero: Slime + sword",
            HeroEvolution.Helmet => "Hero: Slime + helm",
            _ => "Hero: Blue slime"
        };

        var biomeName = BiomeAt.DisplayNameRu(BiomeAt.FromMeters(_score));

        if (_font != null)
        {
            _spriteBatch.DrawString(_font, biomeName, new Vector2(14, 12), new Color(255, 245, 200));
            _spriteBatch.DrawString(_font, coinText, new Vector2(14, 36), new Color(255, 220, 80));
            _spriteBatch.DrawString(_font, formText, new Vector2(14, 60), new Color(140, 200, 255));
            _spriteBatch.DrawString(_font, $"\u0420\u0435\u043a\u043e\u0440\u0434: {FormatMeters(_highScore)}", new Vector2(14, 84),
                new Color(255, 255, 255, 200));
            return;
        }

        DrawCoinIcon(new Vector2(14, 12));
        DrawRect(new Rectangle(38, 14, 8 + _coinCount * 10, 8), new Color(255, 200, 60));
        DrawRect(new Rectangle(14, 34, 100, 6), new Color(140, 200, 255));
    }

    private void DrawMetersCounter()
    {
        var meters = MetersClimbed;
        var useFont = _font != null;
        var mainText = FormatMetersHud(meters, useFont);
        var panelW = Math.Max(140, mainText.Length * 10 + 36);
        var panel = new Rectangle(ScreenWidth / 2 - panelW / 2, 10, panelW, 44);
        DrawRect(panel, new Color((byte)10, (byte)8, (byte)24, (byte)190));
        DrawRect(new Rectangle(panel.X, panel.Y, panel.Width, 3), new Color(120, 180, 255));

        if (useFont)
        {
            var label = "\u0412\u042b\u0421\u041e\u0422\u0410";
            var labelSize = _font.MeasureString(label);
            _spriteBatch.DrawString(_font, label, new Vector2(panel.Center.X - labelSize.X / 2f, panel.Y + 4),
                new Color(160, 200, 255));

            var textSize = _font.MeasureString(mainText);
            _spriteBatch.DrawString(
                _font,
                mainText,
                new Vector2(panel.Center.X - textSize.X / 2f, panel.Y + 20),
                new Color(255, 245, 200));
            return;
        }

        DrawBlockText("HEIGHT", new Vector2(panel.Center.X, panel.Y + 8), new Color(160, 200, 255), 2, centered: true);
        DrawBlockText(mainText, new Vector2(panel.Center.X, panel.Y + 28), new Color(255, 245, 200), 3, centered: true);
    }

    private static string FormatMeters(int meters) =>
        meters >= 1000 ? $"{meters} \u043c ({meters / 1000f:0.##} \u043a\u043c)" : $"{meters} \u043c";

    private static string FormatMetersHud(int meters, bool useFont) =>
        useFont ? FormatMeters(meters) : (meters >= 1000 ? $"{meters} M" : $"{meters} M");

    private void DrawCoinIcon(Vector2 position)
    {
        var rect = new Rectangle((int)position.X, (int)position.Y, 18, 18);
        DrawRect(rect, new Color(255, 200, 40));
        DrawRect(new Rectangle(rect.X + 5, rect.Y + 4, 8, 10), new Color(255, 240, 140));
    }

    private void DrawSuperJumpBanner()
    {
        var t = _superJumpBanner / 1.4f;
        var alpha = (byte)(220 * t);
        var text = "ARCANE LEAP!";
        var color = new Color((byte)255, (byte)230, (byte)80, alpha);

        if (_font != null)
        {
            var size = _font.MeasureString(text);
            _spriteBatch.DrawString(
                _font,
                text,
                new Vector2(ScreenWidth / 2f - size.X / 2f, ScreenHeight * 0.22f),
                color);
            return;
        }

        DrawBlockText(text, new Vector2(ScreenWidth / 2f, ScreenHeight * 0.24f), color, 3, centered: true);
    }

    private void DrawSecretKillBanner()
    {
        var t = _secretKillBanner / 1.1f;
        var alpha = (byte)(220 * t);
        var text = "DRAGON SLAYER!";
        var color = new Color((byte)255, (byte)100, (byte)90, alpha);

        if (_font != null)
        {
            var size = _font.MeasureString(text);
            _spriteBatch.DrawString(
                _font,
                text,
                new Vector2(ScreenWidth / 2f - size.X / 2f, ScreenHeight * 0.18f),
                color);
            return;
        }

        DrawBlockText(text, new Vector2(ScreenWidth / 2f, ScreenHeight * 0.2f), color, 3, centered: true);
    }

    private void DrawDarkSoulsDeath()
    {
        if (_score > _highScore)
            _highScore = _score;

        var vignetteAlpha = (byte)(200 * _deathFade);
        DrawRect(new Rectangle(0, 0, ScreenWidth, ScreenHeight), new Color((byte)8, (byte)0, (byte)0, vignetteAlpha));

        var title = DeathMessage;
        var titleAlpha = (byte)(255 * MathF.Pow(_deathFade, 1.6f));
        var titleColor = new Color((byte)139, (byte)0, (byte)0, titleAlpha);
        var glowColor = new Color((byte)60, (byte)0, (byte)0, (byte)(titleAlpha / 2));

        if (_font != null)
        {
            var scale = 1.6f + 0.25f * (1f - _deathFade);
            var size = _font.MeasureString(title) * scale;
            var pos = new Vector2(ScreenWidth / 2f - size.X / 2f, ScreenHeight * 0.38f - size.Y / 2f);
            _spriteBatch.DrawString(_font, title, pos + new Vector2(2, 2), glowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            _spriteBatch.DrawString(_font, title, pos, titleColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
        else
        {
            DrawBlockText(title, new Vector2(ScreenWidth / 2f + 4, ScreenHeight * 0.4f + 4), glowColor, 6, centered: true);
            DrawBlockText(title, new Vector2(ScreenWidth / 2f, ScreenHeight * 0.4f), titleColor, 6, centered: true);
        }

        if (_deathFade < 0.55f)
            return;

        var hintAlpha = (byte)(180 * (_deathFade - 0.55f) / 0.45f);
        var hint = "Space / Enter — retry";
        var hintColor = new Color((byte)160, (byte)150, (byte)140, hintAlpha);

        if (_font != null)
        {
            var hintSize = _font.MeasureString(hint);
            _spriteBatch.DrawString(
                _font,
                hint,
                new Vector2(ScreenWidth / 2f - hintSize.X / 2f, ScreenHeight * 0.58f),
                hintColor);
            var scoreLine = $"{FormatMeters(_score)}   \u0420\u0435\u043a\u043e\u0440\u0434: {FormatMeters(_highScore)}";
            var scoreSize = _font.MeasureString(scoreLine);
            _spriteBatch.DrawString(
                _font,
                scoreLine,
                new Vector2(ScreenWidth / 2f - scoreSize.X / 2f, ScreenHeight * 0.64f),
                new Color((byte)120, (byte)110, (byte)100, hintAlpha));
        }
    }

    private void DrawBlockText(string text, Vector2 center, Color color, int pixelSize, bool centered)
    {
        const int glyphW = 5;
        const int glyphH = 7;
        const int spacing = 1;
        var totalW = text.Length * (glyphW + spacing) - spacing;
        var startX = centered ? center.X - totalW * pixelSize / 2f : center.X;
        var startY = center.Y - glyphH * pixelSize / 2f;

        for (var i = 0; i < text.Length; i++)
        {
            var glyph = GetGlyph(text[i]);
            var ox = startX + i * (glyphW + spacing) * pixelSize;
            for (var row = 0; row < glyphH; row++)
            {
                for (var col = 0; col < glyphW; col++)
                {
                    if ((glyph[row] & (1 << (glyphW - 1 - col))) == 0)
                        continue;

                    DrawRect(
                        new Rectangle(
                            (int)(ox + col * pixelSize),
                            (int)(startY + row * pixelSize),
                            pixelSize,
                            pixelSize),
                        color);
                }
            }
        }
    }

    private static int[] GetGlyph(char c) => c switch
    {
        'A' => new[] { 0b01110, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b10001 },
        'C' => new[] { 0b01110, 0b10001, 0b10000, 0b10000, 0b10000, 0b10001, 0b01110 },
        'D' => new[] { 0b11110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b11110 },
        'E' => new[] { 0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b11111 },
        'G' => new[] { 0b01110, 0b10001, 0b10000, 0b10111, 0b10001, 0b10001, 0b01110 },
        'H' => new[] { 0b10001, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b10001 },
        'I' => new[] { 0b01110, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110 },
        'J' => new[] { 0b00111, 0b00010, 0b00010, 0b00010, 0b10010, 0b10010, 0b01100 },
        'M' => new[] { 0b10001, 0b11011, 0b10101, 0b10001, 0b10001, 0b10001, 0b10001 },
        'N' => new[] { 0b10001, 0b11001, 0b10101, 0b10011, 0b10001, 0b10001, 0b10001 },
        'O' => new[] { 0b01110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110 },
        'P' => new[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10000, 0b10000, 0b10000 },
        'R' => new[] { 0b11110, 0b10001, 0b10001, 0b11110, 0b10100, 0b10010, 0b10001 },
        'S' => new[] { 0b01111, 0b10000, 0b10000, 0b01110, 0b00001, 0b00001, 0b11110 },
        'U' => new[] { 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110 },
        'Y' => new[] { 0b10001, 0b10001, 0b01010, 0b00100, 0b00100, 0b00100, 0b00100 },
        '!' => new[] { 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00000, 0b00100 },
        '0' => new[] { 0b01110, 0b10001, 0b10011, 0b10101, 0b11001, 0b10001, 0b01110 },
        '1' => new[] { 0b00100, 0b01100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110 },
        '2' => new[] { 0b01110, 0b10001, 0b00001, 0b00010, 0b00100, 0b01000, 0b11111 },
        '3' => new[] { 0b01110, 0b10001, 0b00001, 0b00110, 0b00001, 0b10001, 0b01110 },
        '4' => new[] { 0b00010, 0b00110, 0b01010, 0b10010, 0b11111, 0b00010, 0b00010 },
        '5' => new[] { 0b11111, 0b10000, 0b11110, 0b00001, 0b00001, 0b10001, 0b01110 },
        '6' => new[] { 0b00110, 0b01000, 0b10000, 0b11110, 0b10001, 0b10001, 0b01110 },
        '7' => new[] { 0b11111, 0b00001, 0b00010, 0b00100, 0b01000, 0b01000, 0b01000 },
        '8' => new[] { 0b01110, 0b10001, 0b10001, 0b01110, 0b10001, 0b10001, 0b01110 },
        '9' => new[] { 0b01110, 0b10001, 0b10001, 0b01111, 0b00001, 0b00010, 0b01100 },
        '(' => new[] { 0b00010, 0b00100, 0b01000, 0b01000, 0b01000, 0b00100, 0b00010 },
        ')' => new[] { 0b01000, 0b00100, 0b00010, 0b00010, 0b00010, 0b00100, 0b01000 },
        ' ' => new[] { 0, 0, 0, 0, 0, 0, 0 },
        _ => new[] { 0b11111, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b11111 },
    };

    private void DrawRect(Rectangle rect, Color color)
    {
        _spriteBatch.Draw(_pixel, rect, color);
    }

    private bool WasPressed(KeyboardState current, Keys key) =>
        current.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);
}
