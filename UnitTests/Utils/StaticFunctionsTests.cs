using System;
using DemoForum.Utils;
using NUnit.Framework;

namespace DemoForumTests.Utils;

public class StaticFunctionsTests
{
    private const string DefaultSuffix = " ...";

    [Test]
    [TestCase("字", 20, DefaultSuffix, "字")]
    [TestCase("字字字", 2, DefaultSuffix, "字字" + DefaultSuffix)]
    [TestCase("我我aaa", 4, "b", "我我aab")]
    [TestCase("我我aaa", 5, "b", "我我aaa")]
    public void ShortenToPreview_ShouldTurnLongStringIntoPreviewText_AndReturn(string s, int len, string suffix, string expected)
    {
        // Act
        string actual = s.ShortenToPreview(len, suffix);

        // Assert
        Assert.AreEqual(expected, actual);
    }
    
    [Test]
    [TestCase("abcddd")]
    [TestCase("abcdefghijklmnopqrstuvwxyz")]
    [TestCase("一二三")]
    [TestCase("一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十")]
    public void ShortenToDefault_ShouldTurnLongStringIntoPreviewTextByDefaultFormat_AndReturn(string s)
    {
        // Arrange
        string expected = Arrange_FormatByDefaultLengthAndSuffix(s);
        
        // Act
        string actual = s.ShortenToPreviewDefault();

        // Assert
        Assert.AreEqual(expected, actual);
    }
    
    [Test]
    [TestCase("字", 20, "字")]
    [TestCase("字字字", 2, "字字" + DefaultSuffix)]
    [TestCase("我我aaa", 4, "我我aa" + DefaultSuffix)]
    [TestCase("我我aaa", 5, "我我aaa")]
    public void ShortenToDefault_ShouldTurnLongStringIntoPreviewTextByDefaultFormat_AndReturn(string s, int len, string expected)
    {
        // Act
        string actual = s.ShortenToPreviewDefault(len);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    private static string Arrange_FormatByDefaultLengthAndSuffix(string actual)
    {
        return actual.Length > 20 ? string.Concat(actual.AsSpan(0, 20), DefaultSuffix) : actual;
    }
}