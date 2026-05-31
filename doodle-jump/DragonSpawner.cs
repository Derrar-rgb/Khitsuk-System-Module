using Microsoft.Xna.Framework;

namespace DoodleJump;

public sealed class DragonSpawner
{
    private static readonly int[] MilestonesMeters = { 1000, 10000, 30000, 50000 };

    private readonly HashSet<int> _spawned = new();
    private readonly Random _random = new();

    public static int DragonCount => MilestonesMeters.Length;

    public void Reset() => _spawned.Clear();

    /// <summary>Spawns the dragon for cheat slot (0..3) on a platform above the player.</summary>
    public Dragon? CheatSpawnForSlot(
        int slot,
        IReadOnlyList<Platform> platforms,
        List<Dragon> dragons,
        float playerY)
    {
        if (slot < 0 || slot >= MilestonesMeters.Length)
            return null;

        var platform = PickPlatformAbovePlayer(platforms, playerY);
        if (platform == null)
            return null;

        var x = platform.Position.X + Platform.Width / 2f - Dragon.Width / 2f;
        var y = platform.Position.Y - Dragon.Height + 4f;
        var dragon = new Dragon(new Vector2(x, y), _random);
        dragons.Add(dragon);
        _spawned.Add(MilestonesMeters[slot]);
        return dragon;
    }

    private static Platform? PickPlatformAbovePlayer(IReadOnlyList<Platform> platforms, float playerY)
    {
        Platform? best = null;
        var bestY = float.MaxValue;

        foreach (var platform in platforms)
        {
            if (!platform.Active)
                continue;

            var dy = platform.Position.Y - (playerY - 120f);
            if (dy > -30f || dy < -300f)
                continue;

            if (platform.Position.Y >= bestY)
                continue;

            bestY = platform.Position.Y;
            best = platform;
        }

        return best;
    }

    public void TrySpawnAtMilestones(
        int scoreMeters,
        IReadOnlyList<Platform> platforms,
        List<Dragon> dragons,
        float playerY)
    {
        foreach (var milestone in MilestonesMeters)
        {
            if (scoreMeters < milestone || _spawned.Contains(milestone))
                continue;

            if (!TrySpawnDragon(platforms, dragons, playerY))
                continue;

            _spawned.Add(milestone);
        }
    }

    private bool TrySpawnDragon(IReadOnlyList<Platform> platforms, List<Dragon> dragons, float playerY)
    {
        Platform? best = null;
        var bestDist = float.MaxValue;

        foreach (var platform in platforms)
        {
            if (!platform.Active)
                continue;

            var dy = platform.Position.Y - (playerY - 220f);
            if (dy > -40f || dy < -320f)
                continue;

            var dist = MathF.Abs(dy);
            if (dist >= bestDist)
                continue;

            bestDist = dist;
            best = platform;
        }

        if (best == null)
            return false;

        var x = best.Position.X + Platform.Width / 2f - Dragon.Width / 2f;
        var y = best.Position.Y - Dragon.Height + 4f;
        dragons.Add(new Dragon(new Vector2(x, y), _random));
        return true;
    }

    public void RemoveBelow(List<Dragon> dragons, float yThreshold) =>
        dragons.RemoveAll(d => !d.Active || d.Position.Y > yThreshold);

    public void Scroll(List<Dragon> dragons, float delta)
    {
        foreach (var dragon in dragons)
        {
            if (dragon.Active)
                dragon.Position.Y += delta;
        }
    }
}
