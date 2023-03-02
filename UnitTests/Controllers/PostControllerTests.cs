using System;
using System.Globalization;
using System.Threading.Tasks;
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
    public async Task EditPostWithModel_WillCheckEditIsCorrect_AndCreateToDBAndRedirectToIndex()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        Mock<INotyfService> mockNotyf = Arrange_Notyf();

        PostController postController = Arrange_Controller(mockRepo, mockNotyf, Arrange_Logger());

        EditPostViewModel mockModel = Arrange_EditPostViewModel_New();
        Post mockEntity = Arrange_Post_FromMockViewModel(mockModel);

        // Act
        IActionResult actual = await Act_EditPost_Post(postController, mockModel);

        // Assert
        Assert_Notyf_Success(mockNotyf);
        Assert_PostRepository_CreatedOnce(mockRepo, mockEntity);
        Assert_RedirectedToIndexAction(actual);
    }

    [Test]
    public async Task EditPostWithModel_WillCheckEditIsInvalid_AndPostBack()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        PostController controller = Arrange_Controller(mockRepo, Arrange_Notyf(), Arrange_Logger());
        EditPostViewModel viewModel = new();
        controller.ModelState.AddModelMockError();

        // Act
        IActionResult actual = await Act_EditPost_Post(controller, viewModel);

        // Assert
        Assert_IsView_EditPost(actual);
        Assert_PostRepository_NeverInteracted(mockRepo);
    }

    [Test]
    [TestCase(PostMode.Edit)]
    [TestCase(PostMode.New)]
    public void EditPost_WillCatchError_AndReturnToIndexWithNotyf(PostMode postMode)
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        EditPostViewModel viewModel;
        switch (postMode)
        {
            case PostMode.New:
                mockRepo.Setup(m => m.Create(It.IsAny<Post>())).Throws<Exception>();
                viewModel = Arrange_EditPostViewModel_New();
                break;
            case PostMode.Edit:
                mockRepo.Setup(m => m.Update(It.IsAny<int>(), It.IsAny<Post>())).Throws<Exception>();
                viewModel = Arrange_EditPostViewModel_Edit();
                break;
            default:
                Assert.Fail();
                return;
        }
        
        Mock<INotyfService> mockNotyf = Arrange_Notyf();
        PostController controller = Arrange_Controller(mockRepo, mockNotyf, Arrange_Logger());

        // Act
        // Assert
        Assert.DoesNotThrowAsync(() => controller.EditPost(viewModel));
        Assert_Notyf_ErrorAtLeastOnce(mockNotyf);
    }

    private static void Assert_Notyf_ErrorAtLeastOnce(Mock<INotyfService> mockNotyf)
    {
        mockNotyf.Verify(m => m.Error(It.IsAny<string>(), default), Times.AtLeastOnce);
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

    [Test]
    [TestCase(1, "標題", "內容")]
    [TestCase(3, "標題2", "內容2")]
    public async Task Read_WillQueryForPost_AndReturnCorrectPostViewModel(int id, string title, string content)
    {
        // Arrange
        Post mockPost = Arrange_Post(title, content);
        Mock<IPostRepository> mockRepo = Arrange_ReadFromRepo_WillReturnPost(id, mockPost);
        PostController controller = Arrange_Controller(mockRepo, Arrange_Notyf(), Arrange_Logger());

        // Act
        IActionResult actual = await controller.Read(id);

        // Assert
        Assert_Read_RepoCalledOnce(id, mockRepo);
        PostViewModel viewModel = Assert_Read_IsPostViewModel(actual);
        Assert_Read_ResultIsSameAsEntity(title, content, viewModel, mockPost);
    }

    [Test]
    [TestCase(1)]
    [TestCase(69)]
    [TestCase(100)]
    public async Task Read_WillQueryForPost_AndReturnDefaultModelIfNotFound(int id)
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_ReadFromRepo_WillReturnPost(id, null);
        PostController controller = Arrange_Controller(mockRepo, Arrange_Notyf(), Arrange_Logger());
        
        // Act
        IActionResult actual = await controller.Read(id);
        
        // Assert
        Assert_Read_RepoCalledOnce(id, mockRepo);
        Assert_Read_ViewModelIsNull(actual);
    }

    private static void Assert_Read_ViewModelIsNull(IActionResult actual)
    {
        Assert.IsAssignableFrom<ViewResult>(actual);
        ViewResult viewResult = (ViewResult)actual;
        Assert.IsNull(viewResult.Model);
    }

    private static void Assert_Read_ResultIsSameAsEntity(string title, string content, PostViewModel viewModel,
        Post mockPost)
    {
        Assert.AreEqual(title, viewModel.Title);
        Assert.AreEqual(content, viewModel.Content);
        Assert.AreEqual(mockPost.CreatedTime.ToString(CultureInfo.CurrentCulture), viewModel.CreatedTime);
    }

    private static PostViewModel Assert_Read_IsPostViewModel(IActionResult actual)
    {
        Assert.IsAssignableFrom<ViewResult>(actual);
        ViewResult viewResult = (ViewResult)actual;
        Assert.IsNotNull(viewResult.Model);
        Assert.IsAssignableFrom<PostViewModel>(viewResult.Model);
        PostViewModel viewModel = (PostViewModel)viewResult.Model!;
        return viewModel;
    }

    private Post Arrange_Post(string title, string content)
    {
        return new Post
        {
            Title = title,
            Content = content,
            CreatedTime = DateTime.Now
        };
    }

    private static void Assert_Read_RepoCalledOnce(int id, Mock<IPostRepository> mockRepo)
    {
        mockRepo.Verify(m => m.Read(id), Times.Once);
    }

    private static Mock<IPostRepository> Arrange_ReadFromRepo_WillReturnPost(int id, Post? post)
    {
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        mockRepo.Setup(m => m.Read(id)).ReturnsAsync(post);

        return mockRepo;
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
    
    private static EditPostViewModel Arrange_EditPostViewModel_Edit()
    {
        EditPostViewModel mockModel = new()
        {
            PostTitle = MockPostTitle,
            PostContent = MockPostContent,
            PostMode = PostMode.Edit
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

    private static async Task<IActionResult> Act_EditPost_Post(PostController controller, EditPostViewModel viewModel)
    {
        return await controller.EditPost(viewModel);
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