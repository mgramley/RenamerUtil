using Shouldly;

namespace RenamerUtil.Tests;

public class RenamerIntegrationTests : IDisposable
{
    private readonly string _dir;

    public RenamerIntegrationTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "RenamerUtilTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    private void Touch(string name) => File.WriteAllText(Path.Combine(_dir, name), string.Empty);
    private bool Exists(string name) => File.Exists(Path.Combine(_dir, name));
    private string Read(string name) => File.ReadAllText(Path.Combine(_dir, name));

    [Fact]
    public void RenameTv_NumbersFilesSequentiallyInAlphabeticalOrder()
    {
        Touch("a.mkv");
        Touch("b.mkv");
        Touch("c.mkv");

        new Renamer(_dir).RenameTv("MyShow", season: 1, startEpisode: 1, keepOriginal: false);

        Exists("MyShow - s01e01.mkv").ShouldBeTrue();
        Exists("MyShow - s01e02.mkv").ShouldBeTrue();
        Exists("MyShow - s01e03.mkv").ShouldBeTrue();
        Exists("a.mkv").ShouldBeFalse();
    }

    [Fact]
    public void RenameTv_MapsEachSourceToCorrectEpisodeBasedOnAlphabeticalOrder()
    {
        // Distinct content per file lets us verify which source mapped to which target,
        // not just that the right number of targets were produced. Files are created
        // out of order on purpose; the rename should sort them by stem before numbering.
        File.WriteAllText(Path.Combine(_dir, "gamma.mkv"), "G");
        File.WriteAllText(Path.Combine(_dir, "alpha.mkv"), "A");
        File.WriteAllText(Path.Combine(_dir, "delta.mkv"), "D");
        File.WriteAllText(Path.Combine(_dir, "beta.mkv"),  "B");

        new Renamer(_dir).RenameTv("Show", season: 1, startEpisode: 1, keepOriginal: false);

        Read("Show - s01e01.mkv").ShouldBe("A");
        Read("Show - s01e02.mkv").ShouldBe("B");
        Read("Show - s01e03.mkv").ShouldBe("D");
        Read("Show - s01e04.mkv").ShouldBe("G");
    }

    [Fact]
    public void RenameTv_UsesLexicographicNotNaturalNumericSort()
    {
        // Lexicographic sort of stems puts "ep1" < "ep10" < "ep2" because comparison
        // is character-by-character. Pins this down so a future switch to natural
        // sort would be a deliberate choice, not an accident.
        File.WriteAllText(Path.Combine(_dir, "ep1.mkv"),  "one");
        File.WriteAllText(Path.Combine(_dir, "ep10.mkv"), "ten");
        File.WriteAllText(Path.Combine(_dir, "ep2.mkv"),  "two");

        new Renamer(_dir).RenameTv("Show", season: 1, startEpisode: 1, keepOriginal: false);

        Read("Show - s01e01.mkv").ShouldBe("one");
        Read("Show - s01e02.mkv").ShouldBe("ten");
        Read("Show - s01e03.mkv").ShouldBe("two");
    }

    [Fact]
    public void RenameTv_StartsAtCustomEpisodeNumber()
    {
        Touch("a.mkv");
        Touch("b.mkv");

        new Renamer(_dir).RenameTv("Show", season: 2, startEpisode: 7, keepOriginal: false);

        Exists("Show - s02e07.mkv").ShouldBeTrue();
        Exists("Show - s02e08.mkv").ShouldBeTrue();
    }

    [Fact]
    public void RenameTv_KeepOriginalPreservesScrubbedSourceName()
    {
        Touch("Pilot.mkv");

        new Renamer(_dir).RenameTv("Show", season: 1, startEpisode: 1, keepOriginal: true);

        Exists("Show - s01e01 - Pilot.mkv").ShouldBeTrue();
    }

    [Fact]
    public void RenameMovie_AppendsYearInParens()
    {
        Touch("apollo.13.1995.bluray.mkv");

        new Renamer(_dir).RenameMovie("Apollo 13", year: 1995);

        Exists("Apollo 13 (1995).mkv").ShouldBeTrue();
        Exists("apollo.13.1995.bluray.mkv").ShouldBeFalse();
    }

    [Fact]
    public void AddExtension_AppendsExtensionToEveryFile()
    {
        Touch("video1");
        Touch("video2");

        new Renamer(_dir).AddExtension(".mp4");

        Exists("video1.mp4").ShouldBeTrue();
        Exists("video2.mp4").ShouldBeTrue();
    }

    [Fact]
    public void RemoveStrings_StripsAllPhrasesFromFilenames()
    {
        Touch("[BluRay] My Movie [1080p].mkv");

        new Renamer(_dir).RemoveStrings(["[BluRay] ", " [1080p]"]);

        Exists("My Movie.mkv").ShouldBeTrue();
    }

    [Fact]
    public void DryRun_DoesNotMoveFiles()
    {
        Touch("original.mkv");

        new Renamer(_dir, dryRun: true).RenameTv("Show", season: 1, startEpisode: 1, keepOriginal: false);

        Exists("original.mkv").ShouldBeTrue();
        Exists("Show - s01e01.mkv").ShouldBeFalse();
    }

    [Fact]
    public void DryRun_LogsIntendedRenames()
    {
        Touch("original.mkv");
        var output = new StringWriter();

        new Renamer(_dir, dryRun: true, output: output)
            .RenameTv("Show", season: 1, startEpisode: 1, keepOriginal: false);

        output.ToString().ShouldContain("DRY RUN");
        output.ToString().ShouldContain("Show - s01e01.mkv");
    }

    [Fact]
    public void Move_DoesNotOverwriteExistingTarget()
    {
        // 'preexisting' (no ext) wants to rename to 'preexisting.mp4', which already exists -> skip.
        File.WriteAllText(Path.Combine(_dir, "preexisting.mp4"), "keep me");
        Touch("preexisting");
        var output = new StringWriter();

        new Renamer(_dir, output: output).AddExtension(".mp4");

        Read("preexisting.mp4").ShouldBe("keep me");
        Exists("preexisting").ShouldBeTrue();
        output.ToString().ShouldContain("skip (target exists)");
    }

    [Fact]
    public void PrintFileNames_ListsFilesAlphabeticallyToOutput()
    {
        Touch("zebra.mkv");
        Touch("apple.mp4");
        Touch("middle.avi");
        var output = new StringWriter();

        new Renamer(_dir, output: output).PrintFileNames();

        var lines = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.ShouldBe(["Name: apple.mp4", "Name: middle.avi", "Name: zebra.mkv"]);
    }

    [Fact]
    public void Move_ReportsUnchangedWhenSourceAndTargetAreIdentical()
    {
        Touch("already.mp4");
        var output = new StringWriter();

        // AddExtension(".mp4") on a file ending in .mp4 produces the same name.
        new Renamer(_dir, output: output).AddExtension(".mp4");

        Exists("already.mp4").ShouldBeTrue();
        output.ToString().ShouldContain("unchanged");
    }
}
