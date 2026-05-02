using RenamerUtil;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0 || args[0] is "-h" or "--help")
    {
        PrintUsage();
        return 0;
    }

    var dryRun = false;
    var rest = new List<string>(args.Length);
    foreach (var a in args)
    {
        if (a is "-n" or "--dry-run")
        {
            dryRun = true;
        }
        else
        {
            rest.Add(a);
        }
    }

    if (rest.Count == 0)
    {
        PrintUsage();
        return 0;
    }

    var renamer = new Renamer(Directory.GetCurrentDirectory(), dryRun);
    var cmd = rest[0];

    try
    {
        switch (cmd)
        {
            case "-t":
                renamer.PrintFileNames();
                return 0;

            case "-r":
            case "-rr":
            {
                if (rest.Count < 2)
                {
                    Console.Error.WriteLine($"error: {cmd} requires a prefix");
                    return 1;
                }
                var prefix = rest[1];
                var season = rest.Count > 2 ? int.Parse(rest[2]) : 1;
                var episode = rest.Count > 3 ? int.Parse(rest[3]) : 1;
                renamer.RenameTv(prefix, season, episode, keepOriginal: cmd == "-rr");
                return 0;
            }

            case "-m":
            {
                if (rest.Count < 3)
                {
                    Console.Error.WriteLine("error: -m requires \"<title>\" <year>");
                    return 1;
                }
                if (!int.TryParse(rest[2], out var year))
                {
                    Console.Error.WriteLine($"error: invalid year '{rest[2]}'");
                    return 1;
                }
                renamer.RenameMovie(rest[1], year);
                return 0;
            }

            case "-remove":
                if (rest.Count < 2)
                {
                    Console.Error.WriteLine("error: -remove requires at least one phrase");
                    return 1;
                }
                renamer.RemoveStrings(rest.Skip(1));
                return 0;

            case "-addex":
                if (rest.Count < 2)
                {
                    Console.Error.WriteLine("error: -addex requires an extension");
                    return 1;
                }
                renamer.AddExtension(rest[1]);
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

        Commands:
          -t                                 List files in cwd, sorted (read-only).
          -r  <prefix> [season] [episode]    TV: rename files to "<prefix> - sNNeNN<ext>".
          -rr <prefix> [season] [episode]    TV: same, but keep scrubbed original name as suffix.
          -m  "<title>" <year>               Movie: rename files to "<title> (<year>)<ext>".
          -remove <phrase> [phrase ...]      Remove substring(s) from each filename.
          -addex <.ext>                      Append extension to each file (include the dot).

        Flags:
          -n, --dry-run    Print intended renames without touching files.
          -h, --help       Show this message.

        Notes:
          - season and episode default to 1; episode increments per file (alphabetical order).
          - Existing target names are skipped, never overwritten.
        """);
}
