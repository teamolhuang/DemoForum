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
    private const int TestId = 1;
    private const string TestTitle = "TestTitle";
    private const string TestContent = "TestContent";
    private const string TestTitleChanged = "A new title!";
    private const string TestContentChanged = "A new content??";
    private ForumContext? _forumContext;
    
    private const string MockUsername = "MockUsername";
    private const string MockUserPassword = "MockPassword";

    [SetUp]
    public void SetUp()
    {
        DbContextOptions<ForumContext> inMemoryOptions = new DbContextOptionsBuilder<ForumContext>()
            .UseInMemoryDatabase(DateTime.Now.Ticks.ToString())
            .EnableSensitiveDataLogging()
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
        Assert_IsSameAsDbFirst(actual);
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

    [Test]
    public async Task Update_WillUpdateExistingObject_AndReturnUpdatedEntity()
    {
        // Arrange
        IPostRepository repo = Arrange_GetRepo();
        Arrange_CheckDbEmpty();
        await Arrange_PopulatePost(Arrange_Post());
        Assert_DbHasCorrectNumberOfData(1);

        Post changePost = Arrange_PostChanged();

        // Act
        Post actual = await repo.Update(changePost.Id, changePost);

        // Assert
        Assert_DbHasCorrectNumberOfData(1);
        Assert_IsSameAsDbFirst(actual);
        Update_Assert_UpdatedTimeFilled(actual);
    }

    private static void Update_Assert_UpdatedTimeFilled(Post actual)
    {
        Assert.NotNull(actual.UpdatedTime);
        Assert.Greater(actual.UpdatedTime, actual.CreatedTime);
    }

    [Test]
    public void Update_WillQueryForObjectFirst_AndThrowNullReferenceExceptionIfNotFound()
    {
        // Arrange
        IPostRepository repo = Arrange_GetRepo();
        Arrange_CheckDbEmpty();

        Post changePost = Arrange_PostChanged();

        // Act
        // Assert
        Assert.ThrowsAsync<NullReferenceException>(() => repo.Update(changePost.Id, changePost));
        Assert_DbHasCorrectNumberOfData(0);
    }

    [Test]
    public void Delete_WillQueryFirstAndCheckIsNull_AndThrowNullReferenceException()
    {
        // Arrange
        IPostRepository repo = Arrange_GetRepo();
        Arrange_CheckDbEmpty();

        // Act
        // Assert
        Assert.ThrowsAsync<NullReferenceException>(() => repo.Delete(TestId));
        Arrange_CheckDbEmpty();
    }

    [Test]
    public async Task Delete_WilLQueryFirstAndCheckNotNull_AndRemoveFromDbThenReturnDeletedEntity()
    {
        // Arrange
        IPostRepository repo = Arrange_GetRepo();
        Arrange_CheckDbEmpty();
        Post post = Arrange_Post();
        await Arrange_PopulatePost(post);
        Assert_DbHasCorrectNumberOfData(1);

        // Act
        Post actual = await repo.Delete(TestId);
        
        // Assert
        Arrange_CheckDbEmpty();
        Assert.AreEqual(post, actual);
    }

    private async Task Arrange_PopulatePost(Post arrangePost)
    {
        await _forumContext!.Posts.AddAsync(arrangePost);
        await _forumContext.SaveChangesAsync();
    }

    private void Assert_IsSameAsDbFirst(Post? actual)
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
                CreatedTime = DateTime.Now,
                AuthorId = 1 + i,
                Author = Arrange_MockUser(1 + i, MockUsername),
                Version = ExtensionMethods.GetNowTimestamp()
            });
        }

        _forumContext!.SaveChanges();
    }

    private static User Arrange_MockUser(int mockUserId, string mockUsername)
    {
        return new User
        {
            Id = mockUserId,
            Username = mockUsername,
            Password = MockUserPassword,
            Version = ExtensionMethods.GetNowTimestamp()
        };
    }

    private void Assert_Create_EntityInDbHasColumnsFilled(Post post)
    {
        Post? queried = _forumContext!.Posts
            .FirstOrDefault(p => p.Title.Equals(post.Title) && p.Content.Equals(post.Content));
        Assert.NotNull(queried);
        Assert.NotNull(queried!.Id);
        Assert.NotZero(queried.Id);
        Assert.NotNull(queried.CreatedTime);
        Assert.NotZero(queried.AuthorId);
        Assert.NotNull(queried.Author);
    }
    
    private void Arrange_CheckDbEmpty()
    {
        Assert.IsEmpty(_forumContext!.Posts);
    }

    private static Post Arrange_Post()
    {
        Post post = new()
        {
            Id = TestId,
            Title = TestTitle,
            Content = TestContent,
            AuthorId = 1,
            Author = Arrange_MockUser(1, MockUsername),
            Version = ExtensionMethods.GetNowTimestamp(),
            CreatedTime = DateTime.Now
        };
        return post;
    }
    
    private static Post Arrange_PostChanged()
    {
        Post post = new()
        {
            Id = TestId,
            Title = TestTitleChanged,
            Content = TestContentChanged,
            AuthorId = 1,
            Author = Arrange_MockUser(1, MockUsername),
            Version = ExtensionMethods.GetNowTimestamp(),
            UpdatedTime = DateTime.Now
        };
        return post;
    }
}