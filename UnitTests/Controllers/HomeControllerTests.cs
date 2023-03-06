using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Helpers;
using DemoForum.Controllers;
using DemoForum.Models;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using DemoForum.Utils;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace DemoForumTests.Controllers;

public class HomeControllerTests
{
    private const string MockTitleLong = "A title that is very loooooooooooooooooooooooooooooooooooooooooooooong";
    private const string MockTitleShort = "mockTitle";

    private const string MockContentLong = "VeryLooooooooooooooooooooooooooooooooooooooooong";
    private const string MockContentShort = "Not so long";
    
    private const string MockUsername = "MockUsername";
    private const string MockUserPassword = "MockPassword";

    private const int ContentPreviewLength = 40;

    [Test]
    public async Task Index_WillGet100LatestPosts_AndReturnHomeViewWithPosts()
    {
        // Arrange
        List<Post> posts = Index_Arrange_GetListOfPosts(MockTitleLong, MockTitleShort);

        Mock<IPostRepository> mockRepo = Index_Arrange_MockRepo(posts);

        // Act
        IActionResult actual = await Index_Act(mockRepo);

        // Assert
        Index_Assert_MockRepoCalled(mockRepo);

        HomeViewModel viewModel = actual.AssertAsViewModel<HomeViewModel>();
        Index_Assert_PostPreviewProperlyFormatted(viewModel, posts);
    }

    [Test]
    public async Task Index_WillGetNoPosts_AndReturnHomeViewWithMessages()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Index_Arrange_MockPostRepoEmpty();

        // Act
        IActionResult actual = await Index_Act(mockRepo);

        // Assert
        Index_Assert_MockRepoCalled(mockRepo);

        HomeViewModel viewModel = actual.AssertAsViewModel<HomeViewModel>(); 
        Index_Assert_HasNoPostPreview(viewModel);
    }

    [Test]
    public void RedirectToIndex_WillReturnIndex()
    {
        // Arrange
        HomeController homeController = new(Index_Arrange_MockPostRepoEmpty().Object, Arrange_MockNotyf().Object);

        // Act
        IActionResult actual = homeController.RedirectToIndex();

        // Assert
        actual.AssertAsRedirectToActionResult("Index");
    }

    private void Index_Assert_MockRepoCalled(Mock<IPostRepository> mockRepo)
    {
        mockRepo.Verify(m => m.ReadLatest(100), Times.Once);
    }

    private static async Task<IActionResult> Index_Act(Mock<IPostRepository> mockRepo)
    {
        HomeController home = new(mockRepo.Object, Arrange_MockNotyf().Object);
        IActionResult actual = await home.Index();
        return actual;
    }

    private static Mock<INotyfService> Arrange_MockNotyf()
    {
        return new Mock<INotyfService>();
    }
    
    private void Index_Assert_HasNoPostPreview(HomeViewModel viewModel)
    {
        CollectionAssert.IsEmpty(viewModel.Posts);
    }

    private void Index_Assert_PostPreviewProperlyFormatted(HomeViewModel viewModel, List<Post> posts)
    {
        string json = viewModel.ToJson();
        Assert.False(json.Contains(MockTitleLong));
        Assert.True(json.Contains(MockTitleLong.ShortenToPreview()));
        Assert.True(json.Contains(MockTitleShort));

        Assert.False(json.Contains(MockContentLong));
        Assert.True(json.Contains(MockContentLong.ShortenToPreview(ContentPreviewLength)));
        Assert.True(json.Contains(MockContentShort));
        
        Assert.True(json.Contains(MockUsername));

        foreach (Post post in posts)
        {
            Assert.True(json.Contains(post.Id.ToString().PadLeft(7, '0')));
            Assert.True(json.Contains(post.CreatedTime.ToString(CultureInfo.CurrentCulture)));
            Assert.True(json.Contains(post.CommentScore.ToString()));
        }
    }

    private static Mock<IPostRepository> Index_Arrange_MockRepo(List<Post> posts)
    {
        Mock<IPostRepository> mockRepo = new();
        mockRepo.Setup(m => m.ReadLatest(100)).ReturnsAsync(posts);
        return mockRepo;
    }

    private static Mock<IPostRepository> Index_Arrange_MockPostRepoEmpty()
    {
        return new Mock<IPostRepository>();
    }

    private static List<Post> Index_Arrange_GetListOfPosts(string mockTitle1, string mockTitle2)
    {
        Random commentScore = new();
        return new List<Post>
        {
            new()
            {
                Id = 1,
                Title = mockTitle1,
                Content = MockContentLong,
                CreatedTime = DateTime.Now + TimeSpan.FromHours(1),
                AuthorId = 1,
                Author = Arrange_MockUser(1, MockUsername, MockUserPassword),
                CommentScore = commentScore.Next()
            },
            new()
            {
                Id = 2,
                Title = mockTitle2,
                Content = MockContentShort,
                CreatedTime = DateTime.Now,
                AuthorId = 2,
                Author = Arrange_MockUser(2, MockUsername, MockUserPassword),
                CommentScore = commentScore.Next()
            }
        };
    }

    private static User Arrange_MockUser(int mockId, string mockUsername, string mockPassword)
    {
        return new()
        {
            Id = mockId,
            Username = mockUsername,
            Password = mockPassword
        };
    }
}