using System.Text.RegularExpressions;

public static class Utilities
{
    public static string RemoveWhitespacesUsingRegex(string _source)
    {
        return Regex.Replace(_source, @"\s", string.Empty);
    }
}
