internal static class LocalConfiguration
{
    public static void Load(string root)
    {
        var path = Path.Combine(root, ".env");
        if (!File.Exists(path)) return;
        foreach (var line in File.ReadLines(path))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;
            var separator = trimmed.IndexOf('=');
            if (separator <= 0) throw new ArgumentException("Invalid .env format. Use KEY=value without shell expressions.");
            var name = trimmed[..separator].Trim();
            if (name is not ("OPENAI_API_KEY" or "OPENAI_MODEL")) continue;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name))) continue;
            var value = trimmed[(separator + 1)..].Trim();
            if (value.Length >= 2 && ((value[0] == '\"' && value[^1] == '\"') || (value[0] == '\'' && value[^1] == '\'')))
                value = value[1..^1];
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}
