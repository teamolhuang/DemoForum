using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using DemoForum.Controllers;
using DemoForum.Enums;
using DemoForum.Models;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace DemoForumTests.Controllers;

public class CommentControllerTests
{
    private const string MockContent = "Mock comment content";
    private const int MockPostId = 1;
    
    [Test]
    [TestCase(CommentMode.Push)]
    [TestCase(CommentMode.Boo)]
    [TestCase(CommentMode.Natural)]
    public async Task PostComment_WillCheckViewModelIdIsNotNullAndPostExists_AndReturnToReadViewWithSuccessMessage(CommentMode mode)
    {
        // Arrange
        Mock<ICommentRepository> mockCommentRepo = Arrange_MockCommentRepository();
        Mock<IPostRepository> mockPostRepo = Arrange_MockPostRepository();
        Mock<INotyfService> mockNotyf = Arrange_MockNotyf();
        CommentController controller = Arrange_Controller(mockCommentRepo, mockPostRepo, mockNotyf);

        Post mockPost = Arrange_MockPost();
        PostComment_Arrange_MockRepoRead(mockPostRepo, mockPost);
        
        PostViewModel mockPostView = Arrange_PostViewModel();

        // Act

        IActionResult actual = mode switch
        {
            CommentMode.Push => await controller.Push(mockPostView),
            CommentMode.Boo => await controller.Boo(mockPostView),
            CommentMode.Natural => await controller.Natural(mockPostView),
            _ => throw new ArgumentException()
        };

        // Assert
        mockPostRepo.VerifyAll();
        PostComment_Assert_CommentIsCreated(mockCommentRepo, mockPostView);
        Assert_Notyf_SuccessAtLeastOnce(mockNotyf);
        PostComment_Assert_RedirectedToReadAndHasPostId(actual, mockPostView);
    }
    
    [Test]
    [TestCase(CommentMode.Push)]
    [TestCase(CommentMode.Boo)]
    [TestCase(CommentMode.Natural)]
    public void PostComment_WillCheckViewModelIdIsNull_AndThrowNullReferenceException(CommentMode mode)
    {
        // Arrange
        Mock<ICommentRepository> mockCommentRepo = Arrange_MockCommentRepository();
        Mock<IPostRepository> mockPostRepo = Arrange_MockPostRepository();
        CommentController controller = Arrange_Controller(mockCommentRepo, mockPostRepo, Arrange_MockNotyf());

        PostViewModel mockPostView = Arrange_PostViewModelWithIdAsNull();

        // Act

        Assert.ThrowsAsync<NullReferenceException>(() => mode switch
        {
            CommentMode.Push => controller.Push(mockPostView),
            CommentMode.Boo => controller.Boo(mockPostView),
            CommentMode.Natural => controller.Natural(mockPostView),
            _ => throw new ArgumentException()
        });

        // Assert
        mockPostRepo.VerifyNoOtherCalls();
        mockCommentRepo.VerifyNoOtherCalls();
    }
    
    [Test]
    [TestCase(CommentMode.Push)]
    [TestCase(CommentMode.Boo)]
    [TestCase(CommentMode.Natural)]
    public async Task PostComment_WillCheckPostIsNull_AndReturnToIndexWithInformationMessage(CommentMode mode)
    {
        // Arrange
        Mock<ICommentRepository> mockCommentRepo = Arrange_MockCommentRepository();
        Mock<IPostRepository> mockPostRepo = Arrange_MockPostRepository();
        Mock<INotyfService> mockNotyf = Arrange_MockNotyf();
        CommentController controller = Arrange_Controller(mockCommentRepo, mockPostRepo, mockNotyf);

        PostViewModel mockPostView = Arrange_PostViewModel();

        // Act

        IActionResult actual = mode switch
        {
            CommentMode.Push => await controller.Push(mockPostView),
            CommentMode.Boo => await controller.Boo(mockPostView),
            CommentMode.Natural => await controller.Natural(mockPostView),
            _ => throw new ArgumentException()
        };

        // Assert
        mockPostRepo.VerifyAll();
        mockCommentRepo.VerifyNoOtherCalls();
        Assert_Notyf_InformationAtLeastOnce(mockNotyf);
        PostComment_Assert_RedirectedToIndex(actual);
    }

    private static void PostComment_Assert_RedirectedToIndex(IActionResult actual)
    {
        actual.AssertAsRedirectToActionResult("Index", "Home");
    }

    private static void PostComment_Assert_RedirectedToReadAndHasPostId(IActionResult actual, PostViewModel mockPostView)
    {
        RedirectToActionResult actualRedirect = actual.AssertAsRedirectToActionResult("Read", "Post");
        Assert.IsTrue(actualRedirect.RouteValues!.Values.Contains(mockPostView.Id));
    }

    private static void Assert_Notyf_SuccessAtLeastOnce(Mock<INotyfService> mockNotyf)
    {
        mockNotyf.Verify(m => m.Success(It.IsAny<string>(), default), Times.AtLeastOnce);
    }
    
    private static void Assert_Notyf_InformationAtLeastOnce(Mock<INotyfService> mockNotyf)
    {
        mockNotyf.Verify(m => m.Information(It.IsAny<string>(), default), Times.AtLeastOnce);
    }


    private static void PostComment_Assert_CommentIsCreated(Mock<ICommentRepository> mockCommentRepo, PostViewModel mockPostView)
    {
        mockCommentRepo.Verify(m
            => m.Create(It.Is<Comment>(c
                => c.Content == mockPostView.CommentContent)), Times.Once);
    }

    private static void PostComment_Arrange_MockRepoRead(Mock<IPostRepository> mockPostRepo, Post mockPost)
    {
        mockPostRepo.Setup(m => m.Read(MockPostId)).ReturnsAsync(mockPost);
    }

    private static PostViewModel Arrange_PostViewModel()
    {
        PostViewModel mockPostView = new()
        {
            CommentContent = MockContent,
            Id = MockPostId
        };
        return mockPostView;
    }
    
    private static PostViewModel Arrange_PostViewModelWithIdAsNull()
    {
        PostViewModel mockPostView = new()
        {
            CommentContent = MockContent,
            Id = null
        };
        return mockPostView;
    }

    private static Post Arrange_MockPost()
    {
        Post mockPost = new()
        {
            Id = MockPostId
        };
        return mockPost;
    }

    private static CommentController Arrange_Controller(Mock<ICommentRepository> mockCommentRepo
        , Mock<IPostRepository> mockPostRepo
        , Mock<INotyfService> mockNotyf)
    {
        return new (mockCommentRepo.Object
            , mockPostRepo.Object
            , mockNotyf.Object);
    }

    private static Mock<ICommentRepository> Arrange_MockCommentRepository()
    {
        return new();
    }

    private static Mock<IPostRepository> Arrange_MockPostRepository()
    {
        return new();
    }

    private static Mock<INotyfService> Arrange_MockNotyf()
    {
        return new();
    }
}