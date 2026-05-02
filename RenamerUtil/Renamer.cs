using System.Text.RegularExpressions;

namespace RenamerUtil;

public partial class Renamer(string directory, bool dryRun = false, TextWriter? output = null)
{
    private static readonly string[] BadChars =
    [
        "@", "#", "$", "%", "_", "-", "*",
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "(", ")", "'", "."
    ];

    [GeneratedRegex(@"\[?(?:season|episode|480p?|720p?|1080p?|2160p?)\]?", RegexOptions.IgnoreCase)]
    private static partial Regex BadStrings();

    private readonly TextWriter _out = output ?? Console.Out;

    public void PrintFileNames()
    {
        foreach (var f in EnumerateSorted())
        {
            _out.WriteLine("Name: " + f.Name);
        }
    }

    public void RenameTv(string prefix, int season, int startEpisode, bool keepOriginal)
    {
        var episode = startEpisode;
        foreach (var f in EnumerateSorted())
        {
            var newName = FormatTvName(GetStem(f), prefix, season, episode, f.Extension, keepOriginal);
            Move(f, newName);
            episode++;
        }
    }

    public void RenameMovie(string title, int year)
    {
        foreach (var f in EnumerateSorted())
        {
            Move(f, FormatMovieName(title, year, f.Extension));
        }
    }

    public void AddExtension(string extension)
    {
        foreach (var f in EnumerateSorted())
        {
            Move(f, GetStem(f) + extension);
        }
    }

    public void RemoveStrings(IEnumerable<string> phrases)
    {
        var phraseList = phrases.ToList();
        foreach (var f in EnumerateSorted())
        {
            var newStem = phraseList.Aggregate(GetStem(f), (acc, p) => acc.Replace(p, string.Empty));
            Move(f, newStem + f.Extension);
        }
    }

    private List<FileInfo> EnumerateSorted()
    {
        return new DirectoryInfo(directory)
            .GetFiles()
            .OrderBy(GetStem)
            .ToList();
    }

    private void Move(FileInfo source, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            _out.WriteLine($"skip (empty target): {source.Name}");
            return;
        }

        var target = Path.Combine(directory, newName);

        if (string.Equals(source.FullName, target, StringComparison.Ordinal))
        {
            _out.WriteLine($"unchanged: {source.Name}");
            return;
        }

        if (File.Exists(target))
        {
            _out.WriteLine($"skip (target exists): {source.Name} -> {newName}");
            return;
        }

        if (dryRun)
        {
            _out.WriteLine($"DRY RUN: {source.Name} -> {newName}");
            return;
        }

        File.Move(source.FullName, target);
        _out.WriteLine($"{source.Name} -> {newName}");
    }

    public static string FormatTvName(string original, string prefix, int season, int episode, string extension, bool keepOriginal)
    {
        var scrubbed = ScrubForTv(original);
        if (!string.IsNullOrEmpty(prefix))
        {
            scrubbed = scrubbed.Replace(prefix, string.Empty).Trim();
        }

        var stem = $"{prefix} - s{season:00}e{episode:00}";
        return keepOriginal && !string.IsNullOrWhiteSpace(scrubbed)
            ? $"{stem} - {scrubbed}{extension}"
            : $"{stem}{extension}";
    }

    public static string FormatMovieName(string title, int year, string extension)
    {
        return $"{title} ({year}){extension}";
    }

    public static string ScrubForTv(string name)
    {
        var s = BadStrings().Replace(name, string.Empty);
        s = BadChars.Aggregate(s, (acc, c) => acc.Replace(c, string.Empty));
        return s.Trim();
    }

    private static string GetStem(FileInfo f) => Path.GetFileNameWithoutExtension(f.Name);
}
