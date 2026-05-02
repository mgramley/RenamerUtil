# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A single-project .NET Framework 4.5 C# console exe (`RenamerUtil.exe`) for batch-renaming files in a directory, originally written for organizing TV-show video files into `<show> - sNNeNN` form. There are no tests, no NuGet dependencies, no build scripts.

## Build

The project uses the legacy (pre-SDK-style) `.csproj` format targeting `v4.5` of .NET Framework — it predates `dotnet build`.

- Windows / Visual Studio: open `RenamerUtil.sln`, or `msbuild RenamerUtil.sln /p:Configuration=Debug` (or `Release`).
- Linux: requires Mono's `msbuild` (`msbuild RenamerUtil.sln`). `dotnet build` will not work without first migrating the csproj to SDK style.

Build output lands in `RenamerUtil/bin/{Debug,Release}/RenamerUtil.exe`.

## Runtime model

The tool always operates on `Directory.GetCurrentDirectory()` — there is no path argument. Every command enumerates files in cwd (non-recursive) and **mutates them in place via `File.Move`**. There is no dry-run flag, so when iterating on rename logic, test in a throwaway directory.

## CLI surface (dispatched on `args[0]` in `Program.cs`)

| Flag | Behavior |
|---|---|
| `-t` | Print filenames in cwd, sorted by name-without-extension. Read-only. |
| `-r <prefix> [season] [episode]` | Rename every file to `"<prefix> - sNNeNN<ext>"`, incrementing `episode` per file. Defaults: `season=1`, `episode=1`. |
| `-rr <prefix> [season] [episode]` | Same as `-r`, but **keeps** the scrubbed original name as a suffix: `"<prefix> - sNNeNN - <scrubbed-original><ext>"`. |
| `-remove <phrase> [phrase2 ...]` | Run `string.Replace(phrase, "")` over every filename for each phrase given. |
| `-addex <ext>` | Append `<ext>` to every filename in cwd (no dot is added — pass `.mp4`, not `mp4`). |

Anything else (including no args) is a no-op.

## Key implementation detail: the scrubbing lists

`Renamer.cs` has two hardcoded `List<string>` fields — `_badChars` and `_badStrings` — that `FormatName` strips from the original filename before assembling the new name in `-r` / `-rr` mode. They drop digits, punctuation (including `.` and `-`), and resolution/season tokens like `1080p`, `Season`, `Episode`. This is why `-r` produces clean `s01e01` names from messy source filenames, and it's the first place to look when the rename output is wrong: any unwanted leftover comes from a missing entry in these lists, and any over-aggressive stripping comes from an entry that's too broad (e.g. stripping all digits means a show named "24" becomes empty).

`-remove` does **not** use these lists — it's literal `string.Replace` only.
