using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using Microsoft.EntityFrameworkCore;
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
    public async Task Create_WillSaveInputIntoDb_AndReturnSavedEntity()
    {
        // Arrange
        IPostRepository repo = Arrange_GetRepo();

        Post post = Arrange_Post();
        Arrange_CheckDbEmpty();

        // Act
        Post actual = await repo.Create(post);

        // Assert
        Assert.AreEqual(post, actual);
        Assert_Create_EntityInDbHasColumnsFilled(post);
    }

    [Test]
    [TestCase(0)]
    [TestCase(10)]
    [TestCase(50)]
    [TestCase(100)]
    public async Task ReadLatest_WillQueryFirstIndicatedAmountOfPosts_AndReturnEntities(int rows)
    {
        // Arrange
        IPostRepository repo = Arrange_GetRepo();
        Arrange_CheckDbEmpty();
        Arrange_PopulatePosts(rows);

        // Act
        IEnumerable<Post> posts = await repo.ReadLatest(rows);

        // Assert
        Assert_DbHasCorrectNumberOfData(rows);
        Assert_InputAndQuery_SameAndSameOrder(posts);
    }

    [Test]
    public async Task Read_WillTryQuery_AndReturnEntityWhenFound()
    {
        // Arrange
        IPostRepository repo = Arrange_GetRepo();
        Arrange_CheckDbEmpty();
        Arrange_PopulatePosts(1);
        
        // Act
        Post? actual = await repo.Read(1);
        
        // Assert
        Assert_DbHasCorrectNumberOfData(1);
        Assert_Read_IsSameAsQueried(actual);
    }

    [Test]
    public async Task Read_WillTryQuery_AndReturnNullWhenNotFound()
    {
        // Arrange
        IPostRepository repo = Arrange_GetRepo();
        Arrange_CheckDbEmpty();

        // Act
        Post? actual = await repo.Read(1);
        
        // Assert
        Assert_DbHasCorrectNumberOfData(0);
        Assert.IsNull(actual);
    }

    private void Assert_Read_IsSameAsQueried(Post? actual)
    {
        Assert.IsNotNull(actual);
        Assert.AreEqual(_forumContext!.Posts.First(), actual);
    }

    private IPostRepository Arrange_GetRepo()
    {
        return new PostRepository(_forumContext!);
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