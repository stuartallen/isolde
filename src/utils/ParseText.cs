public class ParseText
{
    public static List<string> ParseFile(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        string text = File.ReadAllText(fileName)
            .Replace("\r\n", "\n") // Normalize line endings
            .Replace("\r", "\n");

        return text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }
}