using DemoForum.Utils;
using NUnit.Framework;

namespace DemoForumTests.Utils;

public class LinkifyHelperTests
{
    [Test]
    [TestCase("http://www.google.com.tw/", "<a href='http://www.google.com.tw/' target='_blank'>http://www.google.com.tw/</a>")]
    [TestCase("http://www.google.com.tw", "<a href='http://www.google.com.tw' target='_blank'>http://www.google.com.tw</a>")]
    [TestCase("https://www.google.com.tw/abc/", "<a href='https://www.google.com.tw/abc/' target='_blank'>https://www.google.com.tw/abc/</a>")]
    [TestCase("https://www.google.com.tw/abc-def?g=1", "<a href='https://www.google.com.tw/abc-def?g=1' target='_blank'>https://www.google.com.tw/abc-def?g=1</a>")]
    [TestCase("前面有中文http://www.yahoo.com.tw/和連結連在一起", "前面有中文<a href='http://www.yahoo.com.tw/和連結連在一起' target='_blank'>http://www.yahoo.com.tw/和連結連在一起</a>")]
    [TestCase("前面有中文http://www.aaa.com.tw/ 和連結分開", "前面有中文<a href='http://www.aaa.com.tw/' target='_blank'>http://www.aaa.com.tw/</a> 和連結分開")]
    public void Linkify_WillDetectUrlsInText_AndReturnStringWithHtmlTags(string text, string expected)
    {
        // Arrange
        
        
        // Act
        string actual = LinkifyHelper.Linkify(text);
        
        // Assert
        Assert.AreEqual(expected, actual);
    }
}