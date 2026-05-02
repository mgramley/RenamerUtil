using Shouldly;

namespace RenamerUtil.Tests;

public class FormatTests
{
    [Fact]
    public void TvName_ProducesPrefixSeasonEpisodeFormat()
    {
        Renamer.FormatTvName("anything", "Show", season: 1, episode: 1, ".mkv", keepOriginal: false)
            .ShouldBe("Show - s01e01.mkv");
    }

    [Fact]
    public void TvName_PadsSeasonAndEpisodeToTwoDigits()
    {
        Renamer.FormatTvName("x", "S", season: 12, episode: 5, ".mkv", keepOriginal: false)
            .ShouldBe("S - s12e05.mkv");
    }

    [Fact]
    public void TvName_AllowsThreeDigitEpisodeWhenNeeded()
    {
        Renamer.FormatTvName("x", "S", season: 1, episode: 105, ".mkv", keepOriginal: false)
            .ShouldBe("S - s01e105.mkv");
    }

    [Fact]
    public void TvName_KeepsScrubbedOriginalAsSuffixWhenRequested()
    {
        Renamer.FormatTvName("Pilot", "Show", season: 1, episode: 1, ".mkv", keepOriginal: true)
            .ShouldBe("Show - s01e01 - Pilot.mkv");
    }

    [Fact]
    public void TvName_OmitsSuffixEvenWhenKeepRequested_IfScrubReducesOriginalToEmpty()
    {
        Renamer.FormatTvName("1080p", "Show", season: 1, episode: 1, ".mkv", keepOriginal: true)
            .ShouldBe("Show - s01e01.mkv");
    }

    [Fact]
    public void TvName_RemovesPrefixFromScrubbedOriginalToAvoidDuplication()
    {
        Renamer.FormatTvName("Show Pilot", "Show", season: 1, episode: 1, ".mkv", keepOriginal: true)
            .ShouldBe("Show - s01e01 - Pilot.mkv");
    }

    [Fact]
    public void MovieName_ProducesPlexTitleYearFormat()
    {
        Renamer.FormatMovieName("Apollo 13", year: 1995, ".mkv")
            .ShouldBe("Apollo 13 (1995).mkv");
    }

    [Fact]
    public void MovieName_PreservesPunctuationInTitle()
    {
        Renamer.FormatMovieName("M.A.S.H.", year: 1970, ".mkv")
            .ShouldBe("M.A.S.H. (1970).mkv");
    }

    [Fact]
    public void MovieName_PreservesUnicode()
    {
        Renamer.FormatMovieName("Amélie", year: 2001, ".mkv")
            .ShouldBe("Amélie (2001).mkv");
    }
}
