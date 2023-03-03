using System;
using System.Collections.Generic;
using System.Globalization;
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
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DemoForumTests.Controllers;

public class PostControllerTests
{
    private const int MockPostId = 1;
    private const string MockPostTitle = "MockTitle";
    private const string MockPostContent = "MockContent";
    private const string MockPostTitleChanged = "The Title is changed!";
    private const string MockPostContentChanged = "A whole new content!";

    private const int MockSid = 645;
    private const string MockNameIdentifier = "MockUsername";

    private const string MockUsername = "MockUsername";
    private const string MockUserPassword = "MockPassword";
    private const int MockUserId = 1;
    
    [Test]
    public async Task EditPostWithModel_WillCheckNewPostIsCorrect_AndCreateToDBAndRedirectToIndex()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        Mock<INotyfService> mockNotyf = Arrange_Notyf();
        Mock<HttpContext> mockHttpContext = EditPostWithModel_Arrange_HttpContextWithProperClaim();

        PostController postController = Arrange_Controller_WithMockHttpContext(mockRepo, mockNotyf, Arrange_Logger(), mockHttpContext);

        EditorViewModel mockModel = Arrange_EditPostViewModel_New();
        Post mockEntity = Arrange_Post_FromMockViewModel(mockModel, MockSid);

        // Act
        IActionResult actual = await Act_EditPost_Post(postController, mockModel);

        // Assert
        Assert_Notyf_SuccessAtLeastOnce(mockNotyf);
        mockHttpContext.VerifyAll();
        Assert_PostRepository_CreatedOnce(mockRepo, mockEntity);
        actual.AssertAsRedirectToActionResult("Index", "Home");
    }

    private static Mock<HttpContext> EditPostWithModel_Arrange_HttpContextWithProperClaim()
    {
        Mock<HttpContext> mockHttpContext = new();
        mockHttpContext.Setup(m => m.User.Claims).Returns(new List<Claim>
        {
            new(ClaimTypes.Sid, MockSid.ToString()),
            new(ClaimTypes.NameIdentifier, MockNameIdentifier)
        });
        return mockHttpContext;
    }


    [Test]
    public void EditPostWithModel_WillCheckSid_AndShowErrorMessage()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        Mock<INotyfService> mockNotyf = Arrange_Notyf();
        Mock<HttpContext> mockHttpContext = EditPostWithModel_Arrange_HttpContextWithoutSid();

        PostController postController = Arrange_Controller_WithMockHttpContext(mockRepo, mockNotyf, Arrange_Logger(), mockHttpContext);

        EditorViewModel mockModel = Arrange_EditPostViewModel_New();

        // Act
        // Assert
        Assert.DoesNotThrowAsync(() => Act_EditPost_Post(postController, mockModel));
        Assert_PostRepository_NeverInteracted(mockRepo);
        Assert_Notyf_ErrorAtLeastOnce(mockNotyf);
    }

    private static Mock<HttpContext> EditPostWithModel_Arrange_HttpContextWithoutSid()
    {
        Mock<HttpContext> mockHttpContext = new();
        mockHttpContext.Setup(m => m.User.Claims).Returns(new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, MockNameIdentifier)
        });
        return mockHttpContext;
    }

    private PostController Arrange_Controller_WithMockHttpContext(Mock<IPostRepository> mockRepo, Mock<INotyfService> mockNotyf, Mock<ILogger<PostController>> mockLogger, Mock<HttpContext> mockHttpContext)
    {
        PostController postController = new(mockRepo.Object, mockNotyf.Object, mockLogger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            }
        };
        return postController;
    }

    [Test]
    public async Task EditPostWithModel_WillCheckEditIsInvalid_AndPostBack()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        PostController controller = Arrange_Controller(mockRepo, Arrange_Notyf(), Arrange_Logger());
        EditorViewModel viewModel = new();
        controller.ModelState.AddModelMockError();

        // Act
        IActionResult actual = await Act_EditPost_Post(controller, viewModel);

        // Assert
        actual.AssertAsExactViewModel(viewModel, "Editor");
        Assert_PostRepository_NeverInteracted(mockRepo);
    }

    [Test]
    [TestCase(PostMode.Edit)]
    [TestCase(PostMode.New)]
    public void EditPost_WillCatchError_AndReturnToIndexWithNotyf(PostMode postMode)
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        EditorViewModel viewModel;
        switch (postMode)
        {
            case PostMode.Edit:
                mockRepo.Setup(m => m.Update(It.IsAny<int>(), It.IsAny<Post>())).Throws<Exception>();
                viewModel = Arrange_EditPostViewModel_Edit();
                break;
            case PostMode.New:
            default:
                mockRepo.Setup(m => m.Create(It.IsAny<Post>())).Throws<Exception>();
                viewModel = Arrange_EditPostViewModel_New();
                break;
        }
        
        Mock<INotyfService> mockNotyf = Arrange_Notyf();
        Mock<HttpContext> mockHttpContext = EditPostWithModel_Arrange_HttpContextWithProperClaim();
        PostController controller = Arrange_Controller_WithMockHttpContext(mockRepo, mockNotyf, Arrange_Logger(), mockHttpContext);

        // Act
        Assert.DoesNotThrowAsync(() => controller.PostEditResult(viewModel));
        
        // Assert
        mockRepo.VerifyAll();
        Assert_Notyf_ErrorAtLeastOnce(mockNotyf);
    }

    private static void Assert_Notyf_ErrorAtLeastOnce(Mock<INotyfService> mockNotyf)
    {
        mockNotyf.Verify(m => m.Error(It.IsAny<string>(), default), Times.AtLeastOnce);
    }

    [Test]
    public void GetNewEditor_WillCreateView_AndReturnNewMode()
    {
        // Arrange
        PostController controller = Arrange_Controller_Default();

        // Act
        IActionResult actual = controller.GetNewEditor();

        // Assert
        EditorViewModel editorViewModel = actual
            .AssertAsViewResult("Editor")
            .AssertAsViewModel<EditorViewModel>();
        Assert_ViewModel_PostMode(editorViewModel, PostMode.New);
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
        Assert_PostRepository_ReadOnce(id, mockRepo);
        PostViewModel viewModel = actual
            .AssertAsViewModel<PostViewModel>();
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
        Assert_PostRepository_ReadOnce(id, mockRepo);
        Assert_Read_ViewModelIsNull(actual);
    }

    [Test]
    public async Task ReadFromDeletePostConfirmationViewModel_WillCheckPostIdNull_AndRedirectToIndexWithErrorMessage()
    {
        // Arrange
        Mock<INotyfService> mockNotyf = Arrange_Notyf();
        PostController controller = Arrange_Controller(Arrange_Repo(), mockNotyf, Arrange_Logger());
        DeletePostConfirmationViewModel viewModel = Arrange_DeletePostConfirmationViewModelFromId(null);

        // Act
        IActionResult actual = await controller.Read(viewModel);

        // Assert
        Assert_Notyf_ErrorAtLeastOnce(mockNotyf);
        actual.AssertAsRedirectToActionResult("Index", "Home");
    }
    
    [Test]
    public async Task ReadFromDeletePostConfirmationViewModel_WillCheckPostIdNotNull_AndRedirectToRead()
    {
        // Arrange
        Post mockPost = Arrange_Post(MockPostTitle, MockPostContent);
        Mock<IPostRepository> mockRepo = Arrange_ReadFromRepo_WillReturnPost(mockPost.Id, mockPost);
        PostController controller = Arrange_Controller(mockRepo, Arrange_Notyf(), Arrange_Logger());
        DeletePostConfirmationViewModel viewModel = Arrange_DeletePostConfirmationViewModelFromId(mockPost.Id);

        // Act
        IActionResult actual = await controller.Read(viewModel);

        // Assert
        Assert_PostRepository_ReadOnce(mockPost.Id, mockRepo);
        PostViewModel postViewModel = actual.AssertAsViewResult()
            .AssertAsViewModel<PostViewModel>();
        Assert_Read_ResultIsSameAsEntity(MockPostTitle, MockPostContent, postViewModel, mockPost);
    }

    [Test]
    public async Task EditPost_WillQueryById_AndReturnEditorViewWithContents()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = new();
        PostController postController = Arrange_Controller(mockRepo, Arrange_Notyf(), Arrange_Logger());
        PostViewModel postViewModel = Arrange_PostViewModel();
        Post expectedPost = Arrange_Post(MockPostTitleChanged, MockPostContentChanged);
        EditPost_Arrange_MockRepoReturnsPostById(mockRepo, postViewModel.Id, expectedPost);

        // Act
        IActionResult actual = await postController.EditPost(postViewModel);

        // Assert
        mockRepo.VerifyAll();
        Assert.AreEqual(expectedPost.Id, postViewModel.Id);
        EditPost_Assert_EditorViewModelIsCorrect(actual, expectedPost);
    }

    [Test]
    public void EditPost_WillCheckInputPostId_AndThrowNullReferenceExceptionIfIdIsNull()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = new();
        PostController postController = Arrange_Controller(mockRepo, Arrange_Notyf(), Arrange_Logger());
        PostViewModel postViewModel = Arrange_PostViewModel_NoId();

        // Act
        Assert.ThrowsAsync<NullReferenceException>(() => postController.EditPost(postViewModel));

        // Assert
        mockRepo.VerifyNoOtherCalls();
    }
    
    [Test]
    public void EditPost_WillQueryForPost_AndThrowNullReferenceExceptionIfNotFound()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = new();
        PostController postController = Arrange_Controller(mockRepo, Arrange_Notyf(), Arrange_Logger());
        PostViewModel postViewModel = Arrange_PostViewModel();
        EditPost_Arrange_MockRepoReturnsPostById(mockRepo, postViewModel.Id, null);
        
        // Act
        Assert.ThrowsAsync<NullReferenceException>(() => postController.EditPost(postViewModel));

        // Assert
        mockRepo.VerifyAll();
    }

    [Test]
    public void DeletePostConfirmation_WillCheckPostIdIsNull_AndReturnToIndexWithErrorMessage()
    {
        // Arrange
        Mock<INotyfService> mockNotyf = Arrange_Notyf();
        PostController controller = Arrange_Controller(Arrange_Repo(), mockNotyf, Arrange_Logger());
        PostViewModel postViewModel = Arrange_PostViewModel_WithoutId();
        
        // Act
        IActionResult actual = controller.DeletePostConfirmation(postViewModel);

        // Assert
        Assert_Notyf_ErrorAtLeastOnce(mockNotyf);
        actual.AssertAsRedirectToActionResult("Index", "Home");
    }

    [Test]
    public void DeletePostConfirmation_WillCheckPostId_AndReturnViewWithId()
    {
        // Arrange
        PostController controller = Arrange_Controller_Default();
        PostViewModel postViewModel = Arrange_PostViewModel();
        DeletePostConfirmationViewModel deletePostConfirmationViewModel 
            = Arrange_DeletePostConfirmationViewModelFromPostView(postViewModel);
        
        // Act
        IActionResult actual = controller.DeletePostConfirmation(postViewModel);

        // Assert
        DeletePostConfirmationViewModel actualModel = actual.AssertAsViewResult()
            .AssertAsViewModel<DeletePostConfirmationViewModel>();
        Assert.AreEqual(deletePostConfirmationViewModel.PostId, actualModel.PostId);
    }

    [Test]
    public async Task Delete_WillCheckPostIdIsNotNull_AndExecuteDeleteThenReturnToIndexWithSuccessMessage()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        Mock<INotyfService> mockNotyf = Arrange_Notyf();
        PostController controller = Arrange_Controller(mockRepo, mockNotyf, Arrange_Logger());
        DeletePostConfirmationViewModel model = Arrange_DeletePostConfirmationViewModelFromId(MockPostId);
        
        // Act
        IActionResult actual = await controller.Delete(model);

        // Assert
        Assert_PostRepository_DeletedOnce(mockRepo);
        Assert_Notyf_SuccessAtLeastOnce(mockNotyf);
        actual.AssertAsRedirectToActionResult("Index", "Home");
    }
    
    [Test]
    public async Task Delete_WillCheckPostIdIsNull_AndStraightReturnToIndexWithSuccessMessage()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        Mock<INotyfService> mockNotyf = Arrange_Notyf();
        PostController controller = Arrange_Controller(mockRepo, mockNotyf, Arrange_Logger());
        DeletePostConfirmationViewModel model = Arrange_DeletePostConfirmationViewModelFromId(null);
        
        // Act
        IActionResult actual = await controller.Delete(model);

        // Assert
        Assert_PostRepository_NeverInteracted(mockRepo);
        Assert_Notyf_SuccessAtLeastOnce(mockNotyf);
        actual.AssertAsRedirectToActionResult("Index", "Home");
    }
    
    [Test]
    public async Task Delete_WillNotThrowRepositoryNullReferenceException_AndStraightReturnToIndexWithSuccessMessage()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = Arrange_Repo();
        Mock<INotyfService> mockNotyf = Arrange_Notyf();
        PostController controller = Arrange_Controller(mockRepo, mockNotyf, Arrange_Logger());
        DeletePostConfirmationViewModel model = Arrange_DeletePostConfirmationViewModelFromPostView(Arrange_PostViewModel());

        mockRepo.Setup(m => m.Delete((int)model.PostId!)).Throws<NullReferenceException>();
        
        // Act
        IActionResult actual = await controller.Delete(model);

        // Assert
        Assert_PostRepository_DeletedOnce(mockRepo);
        Assert_Notyf_SuccessAtLeastOnce(mockNotyf);
        actual.AssertAsRedirectToActionResult("Index", "Home");
    }

    private static void Assert_PostRepository_DeletedOnce(Mock<IPostRepository> mockRepo)
    {
        mockRepo.Verify(m => m.Delete(MockPostId), Times.Once);
    }

    private static DeletePostConfirmationViewModel Arrange_DeletePostConfirmationViewModelFromId(int? id)
    {
        return new()
        {
            PostId = id
        };
    }


    private static DeletePostConfirmationViewModel Arrange_DeletePostConfirmationViewModelFromPostView(PostViewModel postViewModel)
    {
        return new()
        {
            PostId = postViewModel.Id
        };
    }

    private static PostViewModel Arrange_PostViewModel_WithoutId()
    {
        return new()
        {
            Id = null,
            Title = MockPostTitle,
            Content = MockPostContent,
            CreatedTime = DateTime.Now.ToString(CultureInfo.CurrentCulture),
            AuthorName = MockUsername
        };
    }

    private static void EditPost_Assert_EditorViewModelIsCorrect(IActionResult actual, Post expectedPost)
    {
        EditorViewModel editorViewModel = actual.AssertAsViewModel<EditorViewModel>();
        Assert.AreEqual(expectedPost.Id, editorViewModel.EntityId);
        Assert.AreEqual(MockPostTitleChanged, editorViewModel.PostTitle);
        Assert.AreEqual(MockPostContentChanged, editorViewModel.PostContent);
        Assert.AreEqual(PostMode.Edit, editorViewModel.PostMode);
    }

    private void EditPost_Arrange_MockRepoReturnsPostById(Mock<IPostRepository> mockRepo
        , int? postViewModelId
        , Post? mockPost)
    {
        mockRepo
            .Setup(m => m.Read(postViewModelId ?? -1))
            .ReturnsAsync(mockPost);
    }

    private static PostViewModel Arrange_PostViewModel()
    {
        PostViewModel postViewModel = new()
        {
            Id = MockPostId,
            Title = MockPostTitle,
            Content = MockPostContent,
            AuthorName = MockUsername
        };
        return postViewModel;
    }
    
    private static PostViewModel Arrange_PostViewModel_NoId()
    {
        PostViewModel postViewModel = new()
        {
            Id = null,
            Title = MockPostTitle,
            Content = MockPostContent,
            AuthorName = MockUsername
        };
        return postViewModel;
    }

    private static void Assert_Read_ViewModelIsNull(IActionResult actual)
    {
        Assert.IsNull(actual.AssertAsViewResult().Model);
    }

    private static void Assert_Read_ResultIsSameAsEntity(string title, string content, PostViewModel viewModel,
        Post mockPost)
    {
        Assert.AreEqual(title, viewModel.Title);
        Assert.AreEqual(content, viewModel.Content);
        Assert.AreEqual(mockPost.CreatedTime.ToString(CultureInfo.CurrentCulture), viewModel.CreatedTime);
        Assert.NotNull(mockPost.UpdatedTime);
        DateTime updatedTime = (DateTime)mockPost.UpdatedTime!;
        Assert.AreEqual(updatedTime.ToString(CultureInfo.CurrentCulture), viewModel.UpdatedTime);
    }

    private Post Arrange_Post(string title, string content)
    {
        return new Post
        {
            Id = MockPostId,
            Title = title,
            Content = content,
            CreatedTime = DateTime.Now,
            UpdatedTime = DateTime.Now + TimeSpan.FromDays(1),
            Author = Arrange_User(MockUserId, MockUsername, MockUserPassword),
            AuthorId = MockUserId
        };
    }

    private User Arrange_User(int id, string username, string password)
    {
        return new User
        {
            Id = id,
            Username = username,
            Password = password
        };
    }

    private static void Assert_PostRepository_ReadOnce(int id, Mock<IPostRepository> mockRepo)
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

    private static Post Arrange_Post_FromMockViewModel(EditorViewModel mockModel, int authorId)
    {
        Post mockEntity = new()
        {
            AuthorId = authorId,
            Title = mockModel.PostTitle!,
            Content = mockModel.PostContent!
        };
        return mockEntity;
    }

    private static EditorViewModel Arrange_EditPostViewModel_New()
    {
        EditorViewModel mockModel = new()
        {
            PostTitle = MockPostTitle,
            PostContent = MockPostContent,
            PostMode = PostMode.New
        };
        return mockModel;
    }
    
    private static EditorViewModel Arrange_EditPostViewModel_Edit()
    {
        EditorViewModel mockModel = new()
        {
            PostTitle = MockPostTitle,
            PostContent = MockPostContent,
            PostMode = PostMode.Edit
        };
        return mockModel;
    }

    private void Assert_PostRepository_CreatedOnce(Mock<IPostRepository> mockRepo, Post mockEntity)
    {
        mockRepo.Verify(m =>
                m.Create(It.Is<Post>(p =>
                    p.Title == mockEntity.Title 
                    && p.Content == mockEntity.Content
                    && p.AuthorId == mockEntity.AuthorId))
            , Times.Once);
    }

    private void Assert_Notyf_SuccessAtLeastOnce(Mock<INotyfService> notyf)
    {
        notyf.Verify(m => m.Success(It.IsAny<string>(), default), Times.AtLeastOnce);
    }


    private void Assert_PostRepository_NeverInteracted(Mock<IPostRepository> mockRepo)
    {
        mockRepo.VerifyNoOtherCalls();
    }

    private static async Task<IActionResult> Act_EditPost_Post(PostController controller, EditorViewModel viewModel)
    {
        return await controller.PostEditResult(viewModel);
    }


    private void Assert_ViewModel_PostMode(EditorViewModel editorViewModel, PostMode postMode)
    {
        Assert.AreEqual(postMode, editorViewModel.PostMode);
    }

    private static PostController Arrange_Controller_Default()
    {
        return Arrange_Controller(Arrange_Repo(), Arrange_Notyf(), Arrange_Logger());
    }
}