namespace Haven.Models;

public static class DeviceBrickRegistry
{
    // Was for modular bricks

    public const string FallbackBrickKey = "toggle";

    private static readonly Dictionary<string, string> TypeToBrickKey =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["light"] = "lamp",
            ["lock"] = "lock",
            ["thermostat"] = "thermostat",
            ["climate"] = "climate",
            ["sensor"] = "climate",
            ["curtain"] = "curtain"
        };

    public static string BrickKeyFor(string? type)
    {
        if (TypeToBrickKey.TryGetValue(type, out var key))
        {
            return key;
        }
        return null;
    }

    public static void Register(string deviceType, string brickKey) =>
        TypeToBrickKey[deviceType] = brickKey;
}