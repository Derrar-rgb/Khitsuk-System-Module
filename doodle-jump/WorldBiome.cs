namespace DoodleJump;

public enum WorldBiome
{
    Forest,
    CloudKingdom,
    Space
}

public static class BiomeAt
{
    public const int CloudStartMeters = 10_000;
    public const int SpaceStartMeters = 30_000;

    public static WorldBiome FromMeters(int meters)
    {
        if (meters >= SpaceStartMeters)
            return WorldBiome.Space;
        if (meters >= CloudStartMeters)
            return WorldBiome.CloudKingdom;
        return WorldBiome.Forest;
    }

    public static string DisplayName(WorldBiome biome) => biome switch
    {
        WorldBiome.CloudKingdom => "Cloud Kingdom",
        WorldBiome.Space => "Cosmos",
        _ => "Enchanted Forest"
    };

    public static string DisplayNameRu(WorldBiome biome) => biome switch
    {
        WorldBiome.CloudKingdom => "\u041e\u0431\u043b\u0430\u0447\u043d\u044b\u0439 \u0440\u0430\u0439",
        WorldBiome.Space => "\u041a\u043e\u0441\u043c\u043e\u0441",
        _ => "\u0417\u0430\u0447\u0430\u0440\u043e\u0432\u0430\u043d\u043d\u044b\u0439 \u043b\u0435\u0441"
    };
}
