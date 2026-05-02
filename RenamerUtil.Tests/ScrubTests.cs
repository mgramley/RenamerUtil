using Shouldly;

namespace RenamerUtil.Tests;

public class ScrubTests
{
    [Theory]
    [InlineData("Season")]
    [InlineData("season")]
    [InlineData("SEASON")]
    [InlineData("Episode")]
    [InlineData("episode")]
    [InlineData("EPISODE")]
    public void StripsSeasonAndEpisodeWordsRegardlessOfCase(string input)
    {
        Renamer.ScrubForTv(input).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("1080p")]
    [InlineData("1080P")]
    [InlineData("[1080p]")]
    [InlineData("[1080P]")]
    [InlineData("720p")]
    [InlineData("[720p]")]
    [InlineData("480p")]
    [InlineData("[480p]")]
    [InlineData("2160p")]
    [InlineData("[2160p]")]
    public void StripsResolutionTagsWithOrWithoutBrackets(string input)
    {
        Renamer.ScrubForTv(input).ShouldBeEmpty();
    }

    [Fact]
    public void StripsAllDigits()
    {
        Renamer.ScrubForTv("abc123def").ShouldBe("abcdef");
    }

    [Fact]
    public void StripsPunctuationCharacters()
    {
        Renamer.ScrubForTv("a@b#c$d%e_f-g*h(i)j'k.l").ShouldBe("abcdefghijkl");
    }

    [Fact]
    public void TrimsLeadingAndTrailingWhitespace()
    {
        Renamer.ScrubForTv("   hello   ").ShouldBe("hello");
    }

    [Fact]
    public void CleansMessyTorrentStyleFilename()
    {
        // Stripping season/episode words + 1080p + digits + dots
        Renamer.ScrubForTv("My.Show.S01E01.1080p.BluRay")
            .ShouldBe("MyShowSEBluRay");
    }

    [Fact]
    public void PreservesAlphabeticContent()
    {
        Renamer.ScrubForTv("Pilot").ShouldBe("Pilot");
    }
}
