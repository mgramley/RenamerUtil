# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A small cross-platform .NET 10 console exe (`RenamerUtil`) for batch-renaming files in a directory, used to turn blu-ray / DVD / 4K rip filenames into Plex-friendly TV (`Show - sNNeNN`) and movie (`Title (Year)`) names. Two projects: `RenamerUtil/` (the CLI) and `RenamerUtil.Tests/` (xunit + Shouldly + NSubstitute).

## Build & test

- `dotnet build` — builds both projects
- `dotnet test` — runs the xunit suite (~40 tests, sub-second)
- `dotnet run --project RenamerUtil -- <args>` — run the CLI without leaving the repo
- The standalone binary lands at `RenamerUtil/bin/Debug/net10.0/RenamerUtil` and runs directly (a `runtimeconfig.json` is emitted alongside it)
- For a single redistributable file: `dotnet publish RenamerUtil -c Release -r linux-x64 --self-contained`

## Runtime model (read this before changing rename logic)

`Renamer` always operates on the directory passed to its constructor — `Program.cs` passes `Directory.GetCurrentDirectory()`, tests pass a temp dir. Enumeration is non-recursive and sorted alphabetically by filename-without-extension. Every rename routes through `Renamer.Move`, which:

1. Skips when the target name is empty/whitespace.
2. Skips with `unchanged:` when source == target.
3. Skips with `skip (target exists):` when the target file already exists — **the tool never overwrites**.
4. Honours `dryRun` (passed via constructor / `-n` / `--dry-run`) by logging `DRY RUN:` instead of moving.

All output (including from `PrintFileNames`) goes through an injectable `TextWriter` so tests can capture it; the CLI defaults to `Console.Out`.

## CLI surface (`Program.cs`, top-level statements, subcommand-style)

Form: `RenamerUtil <command> [args] [flags]`. Flags are orthogonal — they can appear anywhere in argv and apply to whatever command is running.

| Subcommand | Behavior |
|---|---|
| `list` | List files in cwd, sorted (read-only). |
| `tv <prefix> [season] [episode]` | TV: rename to `"<prefix> - sNNeNN<ext>"`, episode increments per file. Add `-k` / `--keep` to retain the scrubbed original name as a suffix. |
| `movie "<title>" <year>` | Movie: rename to `"<title> (<year>)<ext>"`. |
| `strip <phrase> [phrase ...]` | Literal `string.Replace` of each phrase in every filename. Does **not** use the TV scrub lists. |
| `addext <.ext>` | Append extension to every file in cwd (caller supplies the dot). |

Global flags: `-n` / `--dry-run` (preview), `-h` / `--help` (usage). Unknown commands and missing required args print to stderr and exit 1.

## Key implementation detail: the TV scrub lists

`Renamer.cs` has two static fields that drive how the *original* filename gets cleaned before it's used (in `tv --keep` mode as a suffix, and to remove the prefix from any `tv` output):

- `BadChars` — string array of single-character substrings stripped via repeated `string.Replace`. Includes `0-9`, so any year/number in a source filename gets removed. This is the first place to look when `tv --keep` produces a wrong-looking suffix.
- `BadStrings` — a single `Regex` matching `season`, `episode`, and the `480p` / `720p` / `1080p` / `2160p` resolution tags, optionally bracketed (`[1080p]`), case-insensitive. To add a new scrub token (e.g. `BluRay`, `WEB-DL`, `x264`), extend this regex's alternation.

These only run inside `ScrubForTv` (called from `FormatTvName`). Movie mode (`FormatMovieName`) does no scrubbing — the user passes the title verbatim.

`FormatTvName`, `FormatMovieName`, and `ScrubForTv` are intentionally `public static` so tests can exercise them directly without touching the filesystem.
