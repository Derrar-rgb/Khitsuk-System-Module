using Microsoft.Xna.Framework;

namespace DoodleJump;

public sealed class PlatformSpawner
{
    private readonly Random _random = new();
    private float _nextSpawnY;

    public void Reset(float startY, int count, int screenWidth, int screenHeight, List<Platform> platforms)
    {
        platforms.Clear();
        _nextSpawnY = startY;

        for (var i = 0; i < count; i++)
            Spawn(platforms, screenWidth, screenHeight, forceNormal: i < 4);
    }

    public void EnsurePlatformsAbove(List<Platform> platforms, float highestNeededY, int screenWidth, int screenHeight)
    {
        while (_nextSpawnY > highestNeededY)
            Spawn(platforms, screenWidth, screenHeight);
    }

    public void RemoveBelow(List<Platform> platforms, float yThreshold)
    {
        platforms.RemoveAll(p => p.Position.Y > yThreshold);
    }

    public void Scroll(float delta) => _nextSpawnY += delta;

    public Platform? Spawn(List<Platform> platforms, int screenWidth, int screenHeight, bool forceNormal = false)
    {
        var gap = _random.Next(52, 92);
        _nextSpawnY -= gap;

        var x = _random.Next(0, Math.Max(1, screenWidth - Platform.Width));
        var kind = forceNormal ? PlatformKind.Normal : RollKind();
        var platform = new Platform(new Vector2(x, _nextSpawnY), kind, _random);
        platforms.Add(platform);
        return platform;
    }

    private PlatformKind RollKind()
    {
        var roll = _random.Next(100);
        if (roll < 12)
            return PlatformKind.Breakable;
        if (roll < 28)
            return PlatformKind.Moving;
        return PlatformKind.Normal;
    }
}
