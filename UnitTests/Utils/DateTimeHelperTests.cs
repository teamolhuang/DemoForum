using System;
using DemoForum.Utils;
using NUnit.Framework;

namespace DemoForumTests.Utils;

public class DateTimeHelperTests
{
    [Test]
    public void ToStringForView_WillFormatFromYearToSecond_AndReturnString()
    {
        // Arrange
        DateTime now = DateTime.Now;
        
        // Act
        string actual = now.ToStringForView();

        // Assert
        Assert.AreEqual(now.ToString("yyyy/MM/dd HH:mm:ss"), actual);
    }
}