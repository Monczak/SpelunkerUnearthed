namespace MariEngine.Tiles;

public record struct TileData(string ForegroundColor,
    string BackgroundColor,
    char Character,
    string[] Tags,
    string[] Behaviors,
    TileLightData? Light,
    float LightAttenuation,
    string[] CollisionGroups);