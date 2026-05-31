using Microsoft.Xna.Framework;

namespace DoodleJump;

public sealed class CoinSpawner
{
    private readonly Random _random = new();

    public void Reset(List<Coin> coins, IReadOnlyList<Platform> platforms, int screenWidth)
    {
        coins.Clear();

        var placed = 0;
        foreach (var platform in platforms)
        {
            if (!platform.Active)
                continue;

            if (placed >= 2)
                break;

            if (_random.Next(100) < 28)
            {
                coins.Add(CreateOnPlatform(platform));
                placed++;
            }
        }
    }

    public void TrySpawnOnPlatform(List<Coin> coins, Platform platform)
    {
        if (!platform.Active)
            return;

        if (_random.Next(100) < 10)
            coins.Add(CreateOnPlatform(platform));
    }

    public void RemoveBelow(List<Coin> coins, float yThreshold) =>
        coins.RemoveAll(c => c.Collected || c.Position.Y > yThreshold);

    public void Scroll(List<Coin> coins, float delta)
    {
        foreach (var coin in coins)
        {
            if (!coin.Collected)
                coin.Position.Y += delta;
        }
    }

    private Coin CreateOnPlatform(Platform platform)
    {
        var x = platform.Position.X + Platform.Width / 2f - Coin.Size / 2f;
        var y = platform.Position.Y - Coin.Size - 6f;
        return new Coin(new Vector2(x, y), (float)_random.NextDouble() * MathF.Tau);
    }
}
