namespace LegalQuorum.Tests.Helpers;

public static class TempFile
{
    public static string Create(string contents, string? extension = ".csv")
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
        File.WriteAllText(path, contents);
        return path;
    }
}
