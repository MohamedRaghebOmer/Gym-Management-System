namespace GYM.API.Constants;

public static class Paths
{
    public static readonly string BasePath = AppContext.BaseDirectory;

    public static readonly string LogsFolderPath = Path.Combine(
        BasePath,
        "Logs");
}