using AspNetCoreHero.ToastNotification.Abstractions;
using DemoForum.Controllers;
using DemoForum.Enums;
using DemoForum.Models;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DemoForumTests.Controllers;

public class PostControllerTests
{
    private const string MockPostTitle = "MockTitle";
    private const string MockPostContent = "MockContent";
    private const string PostSuccessMessage = "發文成功！";

    [Test]
    public void EditPostWithModel_WillCheckEditIsCorrect_AndCreateToDBAndRedirectToIndex()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        Mock<INotyfService> mockNotyf = Arrange_Notyf();

        PostController postController = Arrange_Controller(mockRepo, mockNotyf, Arrange_Logger());

        EditPostViewModel mockModel = Arrange_EditPostViewModel_New();
        Post mockEntity = Arrange_Post_FromMockViewModel(mockModel);

        // Act
        IActionResult actual = Act_EditPost_Post(postController, mockModel);

        // Assert
        Assert_Notyf_Success(mockNotyf);
        Assert_PostRepository_CreatedOnce(mockRepo, mockEntity);
        Assert_RedirectedToIndexAction(actual);
    }

    [Test]
    public void EditPostWithModel_WillCheckEditIsInvalid_AndPostBack()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        PostController controller = Arrange_Controller(mockRepo, Arrange_Notyf(), Arrange_Logger());
        EditPostViewModel viewModel = new();
        controller.ModelState.AddModelError("test", "test");

        // Act
        IActionResult actual = Act_EditPost_Post(controller, viewModel);

        // Assert
        Assert_IsView_EditPost(actual);
        Assert_PostRepository_NeverInteracted(mockRepo);
    }

    [Test]
    public void GetEditEditor_WillCreateView_AndReturnEditMode()
    {
        // Arrange
        PostController controller = Arrange_Controller_Default();

        // Act
        IActionResult actual = controller.GetEditEditor();

        // Assert
        Assert_IsView_EditPost(actual);
        EditPostViewModel editPostViewModel = Assert_IsViewModel_EditPost(actual);
        Assert_ViewModel_PostMode(editPostViewModel, PostMode.Edit);
    }

    [Test]
    public void GetNewEditor_WillCreateView_AndReturnNewMode()
    {
        // Arrange
        PostController controller = Arrange_Controller_Default();

        // Act
        IActionResult actual = controller.GetNewEditor();

        // Assert
        Assert_IsView_EditPost(actual);
        EditPostViewModel editPostViewModel = Assert_IsViewModel_EditPost(actual);
        Assert_ViewModel_PostMode(editPostViewModel, PostMode.New);
    }

    private static PostController Arrange_Controller(Mock<IPostRepository> mockRepo, Mock<INotyfService> mockNotyf,
        Mock<ILogger<PostController>> mockLogger)
    {
        PostController postController = new(mockRepo.Object, mockNotyf.Object, mockLogger.Object);
        return postController;
    }

    private static Mock<ILogger<PostController>> Arrange_Logger()
    {
        Mock<ILogger<PostController>> mockLogger = new();
        return mockLogger;
    }

    private static Mock<INotyfService> Arrange_Notyf()
    {
        Mock<INotyfService> mockNotyf = new();
        return mockNotyf;
    }

    private static Mock<IPostRepository> Arrange_Repo()
    {
        Mock<IPostRepository> mockRepo = new();
        return mockRepo;
    }

    private static Post Arrange_Post_FromMockViewModel(EditPostViewModel mockModel)
    {
        Post mockEntity = new()
        {
            Title = mockModel.PostTitle!,
            Content = mockModel.PostContent!
        };
        return mockEntity;
    }

    private static EditPostViewModel Arrange_EditPostViewModel_New()
    {
        EditPostViewModel mockModel = new()
        {
            PostTitle = MockPostTitle,
            PostContent = MockPostContent,
            PostMode = PostMode.New
        };
        return mockModel;
    }

    private void Assert_RedirectedToIndexAction(IActionResult actual)
    {
        Assert.IsAssignableFrom<RedirectToActionResult>(actual);
        RedirectToActionResult redirectResult = (RedirectToActionResult)actual;
        Assert.NotNull(redirectResult.ActionName);
        Assert.AreEqual("Index", redirectResult.ActionName!);
        Assert.NotNull(redirectResult.ControllerName);
        Assert.AreEqual("Home", redirectResult.ControllerName!);
    }

    private void Assert_PostRepository_CreatedOnce(Mock<IPostRepository> mockRepo, Post mockEntity)
    {
        mockRepo.Verify(m =>
                m.Create(It.Is<Post>(p =>
                    p.Title == mockEntity.Title && p.Content == mockEntity.Content))
            , Times.Once);
    }

    private void Assert_Notyf_Success(Mock<INotyfService> notyf)
    {
        notyf.Verify(m => m.Success(PostSuccessMessage, default), Times.Once);
    }


    private void Assert_PostRepository_NeverInteracted(Mock<IPostRepository> mockRepo)
    {
        mockRepo.VerifyNoOtherCalls();
    }

    private void Assert_IsView_EditPost(IActionResult actual)
    {
        Assert.IsAssignableFrom<ViewResult>(actual);
        ViewResult viewResult = (ViewResult)actual;
        Assert.AreEqual("EditPost", viewResult.ViewName!);
    }

    private static IActionResult Act_EditPost_Post(PostController controller, EditPostViewModel viewModel)
    {
        return controller.EditPost(viewModel);
    }


    private void Assert_ViewModel_PostMode(EditPostViewModel editPostViewModel, PostMode postMode)
    {
        Assert.AreEqual(postMode, editPostViewModel.PostMode);
    }

    private static EditPostViewModel Assert_IsViewModel_EditPost(IActionResult actual)
    {
        ViewResult viewResult = (ViewResult)actual;
        Assert.NotNull(viewResult.Model);
        Assert.IsAssignableFrom<EditPostViewModel>(viewResult.Model);
        EditPostViewModel editPostViewModel = (EditPostViewModel)((ViewResult)actual).Model!;
        return editPostViewModel;
    }

    private static PostController Arrange_Controller_Default()
    {
        return Arrange_Controller(Arrange_Repo(), Arrange_Notyf(), Arrange_Logger());
    }
}