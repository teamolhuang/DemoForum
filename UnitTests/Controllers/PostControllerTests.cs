using AspNetCoreHero.ToastNotification.Abstractions;
using DemoForum.Controllers;
using DemoForum.Models;
using DemoForum.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DemoForumTests.Controllers;

public class PostControllerTests
{

    [Test]
    public void EditPostWithModel_WillCheckEditIsCorrect_AndUpdateDBAndRedirectToIndex()
    {
        // Arrange
        Mock<IPostRepository> mockRepo = new();
        Mock<INotyfService> mockNotyf = new();
        Mock<ILogger<PostController>> mockLogger = new();

        PostController postController = new(mockRepo.Object, mockNotyf.Object, mockLogger.Object);
        postController.ModelState.Clear();

        EditPostViewModel viewModel = new();

        // Act

        // Assert

    }

    [Test]
    public void EditPostWithModel_WillCheckEditIsInvalid_AndPostBack()
    {
        
    }

    [Test]
    public void EditPost_WillCreateEditorView_AndReturnEditMode()
    {
        
    }

    [Test]
    public void NewPost_WillCreateEditorView_AndReturnNewMode()
    {
        
    }

    [Test]
    public void EditorView_WillCreateByMode_AndReturnEditPostView()
    {
        
    }

    [Test]
    public void EditorView_WillCreateByModel_AndReturnEditPostView()
    {
        
    }
    
    
}