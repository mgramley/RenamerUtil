using RenamerUtil;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
    {
        PrintUsage();
        return 0;
    }

    var dryRun = false;
    var positional = new List<string>(args.Length);
    foreach (var a in args)
    {
        if (a is "-n" or "--dry-run")
        {
            dryRun = true;
        }
        else if (a is "-h" or "--help")
        {
            PrintUsage();
            return 0;
        }
        else
        {
            positional.Add(a);
        }
    }

    if (positional.Count == 0)
    {
        PrintUsage();
        return 0;
    }

    var renamer = new Renamer(Directory.GetCurrentDirectory(), dryRun);
    var cmd = positional[0];
    var rest = positional.Skip(1).ToList();

    try
    {
        switch (cmd)
        {
            case "list":
                renamer.PrintFileNames();
                return 0;

            case "tv":
            {
                var keep = rest.Remove("-k") | rest.Remove("--keep");
                if (rest.Count < 1)
                {
                    Console.Error.WriteLine("error: tv requires a prefix (e.g. tv \"Better Call Saul\" 1 1)");
                    return 1;
                }
                var prefix = rest[0];
                var season = rest.Count > 1 ? int.Parse(rest[1]) : 1;
                var episode = rest.Count > 2 ? int.Parse(rest[2]) : 1;
                renamer.RenameTv(prefix, season, episode, keepOriginal: keep);
                return 0;
            }

            case "movie":
            {
                if (rest.Count < 2)
                {
                    Console.Error.WriteLine("error: movie requires \"<title>\" <year> (e.g. movie \"Apollo 13\" 1995)");
                    return 1;
                }
                if (!int.TryParse(rest[1], out var year))
                {
                    Console.Error.WriteLine($"error: invalid year '{rest[1]}'");
                    return 1;
                }
                renamer.RenameMovie(rest[0], year);
                return 0;
            }

            case "strip":
                if (rest.Count < 1)
                {
                    Console.Error.WriteLine("error: strip requires at least one phrase");
                    return 1;
                }
                renamer.RemoveStrings(rest);
                return 0;

            case "addext":
                if (rest.Count < 1)
                {
                    Console.Error.WriteLine("error: addext requires an extension (e.g. addext .mkv)");
                    return 1;
                }
                renamer.AddExtension(rest[0]);
                return 0;

            default:
                Console.Error.WriteLine($"error: unknown command '{cmd}'");
                PrintUsage();
                return 1;
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"error: {ex.Message}");
        return 1;
    }
}

static void PrintUsage()
{
    Console.WriteLine("""
        RenamerUtil - batch file renamer for Plex-style names. Operates on the current directory (non-recursive).

        Usage: RenamerUtil <command> [args] [flags]

        Commands:
          list                              List files in cwd, sorted (read-only).
          tv <prefix> [season] [episode]    Rename to "<prefix> - sNNeNN<ext>".
                                            Add -k / --keep to retain the scrubbed
                                            original name as a suffix.
          movie "<title>" <year>            Rename to "<title> (<year>)<ext>".
          strip <phrase> [phrase ...]       Remove substring(s) from each filename.
          addext <.ext>                     Append extension to each file (include the dot).

        Flags (apply to any command):
          -n, --dry-run    Print intended renames without touching files.
          -h, --help       Show this help.

        Examples:
          RenamerUtil list
          RenamerUtil tv "Better Call Saul" 1 1 --dry-run
          RenamerUtil tv "Better Call Saul" 1 1 -k
          RenamerUtil movie "Apollo 13" 1995
          RenamerUtil strip "[BluRay]" " 1080p"
          RenamerUtil addext .mkv

        Notes:
          - season and episode default to 1; episode increments per file (alphabetical order).
          - Existing target names are skipped, never overwritten.
        """);
}
