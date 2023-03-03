using System;
using System.Linq;
using System.Threading.Tasks;
using DemoForum.Enums;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using NUnit.Framework;

namespace DemoForumTests.Repositories;

public class CommentRepositoryTests : InMemoryDbSetup
{
    private const int MockUserId = 1;

    private const int MockId = 65535;
    private const string MockContent = "Mock Comment";

    private const int MockPostId = 11037;

    [Test]
    [TestCase(CommentMode.Push)]
    [TestCase(CommentMode.Boo)]
    [TestCase(CommentMode.Natural)]
    public async Task Create_WillReceiveInput_AndInsertIntoDb(CommentMode mode)
    {
        // Arrange
        CommentRepository repository = new(ForumContext);
        Assert_DbIsEmpty();
        Comment mockComment = Arrange_MockComment(mode);

        // Act
        Comment actual = await repository.Create(mockComment);

        // Assert
        Assert.AreEqual(mockComment, actual);
        Assert_DbHasOneRowAndExactly(mockComment);
    }

    private void Assert_DbHasOneRowAndExactly(Comment comment)
    {
        Assert.That(ForumContext.Comments.Count() == 1);
        Assert.AreEqual(comment, ForumContext.Comments.First());
    }

    private void Assert_DbIsEmpty()
    {
        Assert.IsEmpty(ForumContext.Comments);
    }

    private Comment Arrange_MockComment(CommentMode mode)
    {
        Comment mockComment = new()
        {
            AuthorId = MockUserId,
            Content = MockContent,
            CreatedTime = DateTime.Now,
            Id = MockId,
            PostId = MockPostId,
            Type = mode.GetDbEnum(),
            UpdatedTime = null
        };
        return mockComment;
    }
}