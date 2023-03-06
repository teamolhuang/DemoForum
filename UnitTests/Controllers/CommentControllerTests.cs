using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using DemoForum.Controllers;
using DemoForum.Enums;
using DemoForum.Models;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace DemoForumTests.Controllers;

public class CommentControllerTests
{
    private const string MockContent = "Mock comment content";
    private const int MockPostId = 1;

    private const int MockUserId = 404;
    
    [Test]
    [TestCase(CommentMode.Push)]
    [TestCase(CommentMode.Boo)]
    [TestCase(CommentMode.Natural)]
    public async Task PostComment_WillCheckViewModelIdIsNotNullAndPostExistsAndUpdateCommentScore_AndReturnToReadViewWithSuccessMessage(CommentMode mode)
    {
        // Arrange
        Mock<ICommentRepository> mockCommentRepo = Arrange_MockCommentRepository();
        Mock<IPostRepository> mockPostRepo = Arrange_MockPostRepository();
        Mock<IUserRepository> mockUserRepo = Arrange_MockUserRepo();
        Mock<INotyfService> mockNotyf = Arrange_MockNotyf();
        Mock<HttpContext> mockHttpContext = Arrange_MockHttpContext();
        CommentController controller = Arrange_ControllerInjectedHttpContext(mockCommentRepo
            , mockPostRepo
            , mockUserRepo
            , mockNotyf
            , mockHttpContext);

        User mockUser = Arrange_MockUser();
        Arrange_HttpContextReturnsUserClaims(mockHttpContext, mockUser);
        
        Post mockPost = Arrange_MockPost();
        PostComment_Arrange_MockRepoRead(mockPostRepo, mockPost);

        Post mockUpdatedPost = Arrange_MockPost();
        mockUpdatedPost.CommentScore += mode.GetCommentScore();
        PostComment_Arrange_MockPostRepoUpdate(mockPostRepo, mockUpdatedPost);
        PostComment_Arrange_MockUserRepoRead(mockUserRepo, mockUser);

        CommentInputViewModel mockCommentInputView = Arrange_CommentInputView();

        // Act

        IActionResult actual = mode switch
        {
            CommentMode.Push => await controller.Push(mockCommentInputView),
            CommentMode.Boo => await controller.Boo(mockCommentInputView),
            CommentMode.Natural => await controller.Natural(mockCommentInputView),
            _ => throw new ArgumentException()
        };

        // Assert
        mockPostRepo.VerifyAll();
        PostComment_Assert_CommentIsCreated(mockCommentRepo, mockCommentInputView, mockUser);
        Assert_Notyf_SuccessAtLeastOnce(mockNotyf);
        PostComment_Assert_RedirectedToReadAndHasPostId(actual, mockCommentInputView);
    }

    private static void PostComment_Arrange_MockPostRepoUpdate(Mock<IPostRepository> mockPostRepo, Post mockUpdatedPost)
    {
        mockPostRepo
            .Setup(m => m.Update(mockUpdatedPost.Id, GetUpdatedPostComparer(mockUpdatedPost)))
            .ReturnsAsync(mockUpdatedPost);
    }

    private static void PostComment_Arrange_MockUserRepoRead(Mock<IUserRepository> mockUserRepo, User mockUser)
    {
        mockUserRepo.Setup(m => m.Read(MockUserId)).ReturnsAsync(mockUser);
    }

    private static User Arrange_MockUser()
    {
        return new()
        {
            Id = MockUserId
        };
    }

    private static void Arrange_HttpContextReturnsUserClaims(Mock<HttpContext> mockHttpContext, User mockUser)
    {
        mockHttpContext.Setup(m => m.User.Claims).Returns(new List<Claim>
        {
            new(ClaimTypes.Sid, mockUser.Id.ToString())
        });
    }

    private static Mock<HttpContext> Arrange_MockHttpContext()
    {
        return new();
    }

    private static Mock<IUserRepository> Arrange_MockUserRepo()
    {
        return new();
    }

    private static CommentInputViewModel Arrange_CommentInputView()
    {
        CommentInputViewModel mockCommentInputView = new()
        {
            PostId = MockPostId,
            CommentContent = MockContent
        };
        return mockCommentInputView;
    }
    
    private static CommentInputViewModel Arrange_CommentInputViewWithIdAsNull()
    {
        CommentInputViewModel mockCommentInputView = new()
        {
            PostId = null,
            CommentContent = MockContent
        };
        return mockCommentInputView;
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
        CommentController controller = Arrange_Controller(mockCommentRepo
            , mockPostRepo
            , Arrange_MockUserRepo()
            , Arrange_MockNotyf());

        CommentInputViewModel mockCommentInputView = Arrange_CommentInputViewWithIdAsNull();

        // Act

        Assert.ThrowsAsync<NullReferenceException>(() => mode switch
        {
            CommentMode.Push => controller.Push(mockCommentInputView),
            CommentMode.Boo => controller.Boo(mockCommentInputView),
            CommentMode.Natural => controller.Natural(mockCommentInputView),
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
        CommentController controller = Arrange_Controller(mockCommentRepo
            , mockPostRepo
            , Arrange_MockUserRepo()
            , mockNotyf);

        CommentInputViewModel mockCommentView = Arrange_CommentInputView();

        // Act

        IActionResult actual = mode switch
        {
            CommentMode.Push => await controller.Push(mockCommentView),
            CommentMode.Boo => await controller.Boo(mockCommentView),
            CommentMode.Natural => await controller.Natural(mockCommentView),
            _ => throw new ArgumentException()
        };

        // Assert
        mockPostRepo.VerifyAll();
        mockCommentRepo.VerifyNoOtherCalls();
        Assert_Notyf_InformationAtLeastOnce(mockNotyf);
        PostComment_Assert_RedirectedToIndex(actual);
    }
    
    [Test]
    [TestCase(CommentMode.Push)]
    [TestCase(CommentMode.Boo)]
    [TestCase(CommentMode.Natural)]
    public async Task PostComment_WillCheckUserIsNull_AndRedirectToLogoutWithErrorMessage(CommentMode mode)
    {
        // Arrange
        Mock<ICommentRepository> mockCommentRepo = Arrange_MockCommentRepository();
        Mock<IPostRepository> mockPostRepo = Arrange_MockPostRepository();
        Mock<IUserRepository> mockUserRepo = Arrange_MockUserRepo();
        Mock<INotyfService> mockNotyf = Arrange_MockNotyf();
        Mock<HttpContext> mockHttpContext = Arrange_MockHttpContext();
        User mockUser = Arrange_MockUser();
        Arrange_HttpContextReturnsUserClaims(mockHttpContext, mockUser);

        CommentController controller = Arrange_ControllerInjectedHttpContext(mockCommentRepo
            , mockPostRepo
            , mockUserRepo
            , mockNotyf
            , mockHttpContext);

        CommentInputViewModel mockCommentView = Arrange_CommentInputView();

        Post mockPost = Arrange_MockPost();
        PostComment_Arrange_MockRepoRead(mockPostRepo, mockPost);

        mockUserRepo.Setup(m => m.Read(mockUser.Id)).ReturnsAsync(default(User));

        // Act

        IActionResult actual = mode switch
        {
            CommentMode.Push => await controller.Push(mockCommentView),
            CommentMode.Boo => await controller.Boo(mockCommentView),
            CommentMode.Natural => await controller.Natural(mockCommentView),
            _ => throw new ArgumentException()
        };

        // Assert
        mockPostRepo.VerifyAll();
        mockUserRepo.VerifyAll();
        mockCommentRepo.VerifyNoOtherCalls();
        Assert_Notyf_ErrorAtLeastOnce(mockNotyf);
        PostComment_Assert_RedirectedToLogout(actual);
    }
    
    [Test]
    [TestCase(CommentMode.Push, true, false)]
    [TestCase(CommentMode.Boo, true, false)]
    [TestCase(CommentMode.Natural, true, false)]
    [TestCase(CommentMode.Push, false, true)]
    [TestCase(CommentMode.Boo, false, true)]
    [TestCase(CommentMode.Natural, false, true)]
    public async Task PostComment_WillRollBackTransactionIfTransactionFails_AndReturnToReadViewWithSuccessMessage(
        CommentMode mode
        , bool postUpdateSuccess
        , bool commentCreateSuccess)
    {
        // Arrange
        Mock<ICommentRepository> mockCommentRepo = Arrange_MockCommentRepository();
        Mock<IPostRepository> mockPostRepo = Arrange_MockPostRepository();
        Mock<IUserRepository> mockUserRepo = Arrange_MockUserRepo();
        Mock<INotyfService> mockNotyf = Arrange_MockNotyf();
        Mock<HttpContext> mockHttpContext = Arrange_MockHttpContext();
        CommentController controller = Arrange_ControllerInjectedHttpContext(mockCommentRepo
            , mockPostRepo
            , mockUserRepo
            , mockNotyf
            , mockHttpContext);

        User mockUser = Arrange_MockUser();
        Arrange_HttpContextReturnsUserClaims(mockHttpContext, mockUser);
        PostComment_Arrange_MockUserRepoRead(mockUserRepo, mockUser);

        Post mockPost = Arrange_MockPost();
        PostComment_Arrange_MockRepoRead(mockPostRepo, mockPost);

        Post mockUpdatedPost = Arrange_MockPost();
        mockUpdatedPost.CommentScore += mode.GetCommentScore();

        if (postUpdateSuccess)
        {
            PostComment_Arrange_MockPostRepoUpdate(mockPostRepo, mockUpdatedPost);
        }
        else
        {
            PostComment_Arrange_MockPostRepoThrows(mockPostRepo, mockUpdatedPost);
        }

        if (commentCreateSuccess)
        {
            mockCommentRepo.Setup(m => m.Create(It.IsAny<Comment>()))
                .ReturnsAsync((Comment c) => c);
        }
        else
        {
            PostComment_Arrange_MockCommentRepoThrows(mockCommentRepo);
        }
        
        CommentInputViewModel mockCommentInputView = Arrange_CommentInputView();

        // Act

        IActionResult actual = mode switch
        {
            CommentMode.Push => await controller.Push(mockCommentInputView),
            CommentMode.Boo => await controller.Boo(mockCommentInputView),
            CommentMode.Natural => await controller.Natural(mockCommentInputView),
            _ => throw new ArgumentException()
        };

        // Assert
        mockPostRepo.VerifyAll();
        if (postUpdateSuccess)
            mockCommentRepo.VerifyAll();
        Assert_Notyf_ErrorAtLeastOnce(mockNotyf);
        RedirectToActionResult redirect = actual.AssertAsRedirectToActionResult("Read", "Post");
        Assert.IsTrue(redirect.RouteValues!.Contains(new KeyValuePair<string, object?>("Id", mockCommentInputView.PostId)));
    }

    private static void PostComment_Arrange_MockCommentRepoThrows(Mock<ICommentRepository> mockCommentRepo)
    {
        mockCommentRepo.Setup(m => m.Create(It.IsAny<Comment>()))
            .Throws<Exception>();
    }

    private static void PostComment_Arrange_MockPostRepoThrows(Mock<IPostRepository> mockPostRepo, Post mockUpdatedPost)
    {
        mockPostRepo.Setup(m => m.Update(mockUpdatedPost.Id
                , GetUpdatedPostComparer(mockUpdatedPost)))
            .Throws<Exception>();
    }

    private static Post GetUpdatedPostComparer(Post mockUpdatedPost)
    {
        return It.Is<Post>(p => p.Id == mockUpdatedPost.Id 
                                && p.CommentScore == mockUpdatedPost.CommentScore);
    }

    private static void PostComment_Assert_RedirectedToIndex(IActionResult actual)
    {
        actual.AssertAsRedirectToActionResult("Index", "Home");
    }
    
    private static void PostComment_Assert_RedirectedToLogout(IActionResult actual)
    {
        actual.AssertAsRedirectToActionResult("Logout", "User");
    }

    private static void PostComment_Assert_RedirectedToReadAndHasPostId(IActionResult actual, CommentInputViewModel mockPostView)
    {
        RedirectToActionResult actualRedirect = actual.AssertAsRedirectToActionResult("Read", "Post");
        Assert.IsTrue(actualRedirect.RouteValues!.Values.Contains(mockPostView.PostId));
    }

    private static void Assert_Notyf_SuccessAtLeastOnce(Mock<INotyfService> mockNotyf)
    {
        mockNotyf.Verify(m => m.Success(It.IsAny<string>(), default), Times.AtLeastOnce);
    }
    
    private static void Assert_Notyf_InformationAtLeastOnce(Mock<INotyfService> mockNotyf)
    {
        mockNotyf.Verify(m => m.Information(It.IsAny<string>(), default), Times.AtLeastOnce);
    }

    private static void Assert_Notyf_ErrorAtLeastOnce(Mock<INotyfService> mockNotyf)
    {
        mockNotyf.Verify(m => m.Error(It.IsAny<string>(), default), Times.AtLeastOnce);
    }

    private static void PostComment_Assert_CommentIsCreated(Mock<ICommentRepository> mockCommentRepo
        , CommentInputViewModel mockPostView
        , User mockUser)
    {
        mockCommentRepo.Verify(m
            => m.Create(It.Is<Comment>(c
                => c.Content == mockPostView.CommentContent
                && c.AuthorId == mockUser.Id
                && c.Author == mockUser)), Times.Once);
    }

    private static void PostComment_Arrange_MockRepoRead(Mock<IPostRepository> mockPostRepo, Post mockPost)
    {
        mockPostRepo.Setup(m => m.Read(MockPostId)).ReturnsAsync(mockPost);
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
        , Mock<IUserRepository> mockUserRepo
        , Mock<INotyfService> mockNotyf)
    {
        return new (mockCommentRepo.Object
            , mockPostRepo.Object
            , mockUserRepo.Object
            , mockNotyf.Object);
    }
    
    private static CommentController Arrange_ControllerInjectedHttpContext(Mock<ICommentRepository> mockCommentRepo
        , Mock<IPostRepository> mockPostRepo
        , Mock<IUserRepository> mockUserRepo
        , Mock<INotyfService> mockNotyf
        , Mock<HttpContext> mockHttpContext)
    {
        return new(mockCommentRepo.Object
            , mockPostRepo.Object
            , mockUserRepo.Object
            , mockNotyf.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            }
        };
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