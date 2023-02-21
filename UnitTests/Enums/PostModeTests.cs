using DemoForum.Enums;
using NUnit.Framework;

namespace DemoForumTests.Enums;

public class PostModeTests
{
    [Test]
    [TestCase(PostMode.Edit, "編輯")]
    [TestCase(PostMode.New, "發表")]
    public void GetChinese_WillCheckInputEnum_AndReturnChineseDefinition(PostMode postMode, string chinese)
    {
        // Arrange
        
        // Act
        string actual = postMode.GetChinese();

        // Assert
        Assert.AreEqual(chinese, actual);
    }
}