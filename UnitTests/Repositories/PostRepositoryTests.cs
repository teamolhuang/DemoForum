using System;
using System.Collections.Generic;
using System.Linq;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace DemoForumTests.Repositories;

public class PostRepositoryTests
{
    private const string TestTitle = "TestTitle";
    private const string TestContent = "TestContent";
    private ForumContext? _forumContext;
    
    [SetUp]
    public void SetUp()
    {
        DbContextOptions<ForumContext> inMemoryOptions = new DbContextOptionsBuilder<ForumContext>()
            .UseInMemoryDatabase(DateTime.Now.Ticks.ToString())
            .Options;

        _forumContext = new ForumContext(inMemoryOptions);
    }

    [TearDown]
    public void TearDown()
    {
        _forumContext?.Dispose(); 
    }

    [Test]
    public void Create_WillSaveInputIntoDb_AndReturnSavedEntity()
    {
        // Arrange
        PostRepository repo = new(_forumContext!);

        Post post = Arrange_Post();
        Arrange_CheckDbEmpty();

        // Act
        Post actual = repo.Create(post);

        // Assert
        Assert.AreEqual(post, actual);
        Assert_Create_EntityInDbHasColumnsFilled(post);
    }

    [Test]
    [TestCase(10)]
    [TestCase(50)]
    [TestCase(100)]
    public void ReadLatest(int rows)
    {
        // Arrange
        PostRepository repo = new(_forumContext!);
        Arrange_CheckDbEmpty();
        Arrange_PopulatePosts(rows);

        // Act
        IEnumerable<Post> posts = repo.ReadLatest(rows);

        // Assert
        Assert_DbHasCorrectNumberOfData(rows);
        Assert_InputAndQuery_SameAndSameOrder(posts);
    }

    private void Assert_DbHasCorrectNumberOfData(int rows)
    {
        Assert.AreEqual(rows, _forumContext!.Posts.Count());
    }

    private void Assert_InputAndQuery_SameAndSameOrder(IEnumerable<Post> posts)
    {
        CollectionAssert.AreEqual(_forumContext!.Posts.OrderByDescending(p => p.CreatedTime), posts);
    }

    private void Arrange_PopulatePosts(int rows)
    {
        for (int i = 0; i < rows; i++)
        {
            _forumContext!.Posts.Add(new()
            {
                Title = TestTitle + i,
                Content = DateTime.Now.Ticks.ToString(),
                CreatedTime = DateTime.Now
            });
        }

        _forumContext!.SaveChanges();
    }

    private void Assert_Create_EntityInDbHasColumnsFilled(Post post)
    {
        Post? queried = _forumContext!.Posts
            .FirstOrDefault(p => p.Title.Equals(post.Title) && p.Content.Equals(post.Content));
        Assert.NotNull(queried);
        Assert.NotNull(queried!.Id);
        Assert.NotZero(queried.Id);
        Assert.NotNull(queried.CreatedTime);
    }
    
    private void Arrange_CheckDbEmpty()
    {
        Assert.IsEmpty(_forumContext!.Posts);
    }

    private static Post Arrange_Post()
    {
        Post post = new()
        {
            Title = TestTitle,
            Content = TestContent
        };
        return post;
    }
}