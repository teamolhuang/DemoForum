using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using DemoForum.Controllers;
using DemoForum.Models;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;

namespace DemoForumTests.Controllers;

public class UserControllerTests
{
    private const string MockUsername = "TestUsername";
    private const string MockPassword = "3345678999";

    [Test]
    public async Task PostRegister_WillCheckModelState_AndReturnToRegister()
    {
        // Arrange
        UserController controller = Arrange_Controller(Arrange_MockRepo(), Arrange_MockNotyf());
        controller.ModelState.AddModelMockError();

        RegisterViewModel viewModel = Register_Arrange_MockViewModel();
        
        // Act
        IActionResult actual = await controller.PostRegister(viewModel);

        // Assert
        actual.AssertAsExactViewModel(viewModel, "Register");
    }

    [Test]
    public async Task PostRegister_WillCreate_AndReturnToGetLoginWithSuccessMessage()
    {
        // Arrange
        Mock<IUserRepository> mockRepo = Arrange_MockRepo();
        Mock<INotyfService> mockNotyf = Arrange_MockNotyf();
        UserController controller = Arrange_Controller(mockRepo, mockNotyf);

        RegisterViewModel viewModel = Register_Arrange_MockViewModel();
        Arrange_MockRepo_Create(mockRepo, viewModel);

        // Act
        IActionResult actual = await controller.PostRegister(viewModel);

        // Assert
        Assert_PostRepo_CreatedOnce(mockRepo, viewModel);
        Assert_Notyf_SuccessShown(mockNotyf);
        actual.AssertAsRedirectToActionResult("GetLogin");
    }

    [Test]
    public async Task PostRegister_WillCatchError_AndReturnToRegisterWithErrorMessage()
    {
        // Arrange
        Mock<IUserRepository> mockRepo = Arrange_MockRepo();
        Mock<INotyfService> mockNotyf = Arrange_MockNotyf();
        UserController controller = Arrange_Controller(mockRepo, mockNotyf);

        RegisterViewModel viewModel = Register_Arrange_MockViewModel();
        Arrange_MockRepo_CreateWillThrow(mockRepo, viewModel);

        // Act
        IActionResult actual = await controller.PostRegister(viewModel);

        // Assert
        Assert_PostRepo_CreatedOnce(mockRepo, viewModel);
        Assert_Notyf_ErrorShown(mockNotyf);
        actual.AssertAsExactViewModel(viewModel, "Register");
    }

    [Test]
    public async Task PostLogin_WillCheckModelState_AndReturnToLogin()
    {
        // Arrange
        UserController controller = Arrange_Controller(Arrange_MockRepo(), Arrange_MockNotyf());
        controller.ModelState.AddModelMockError();

        LoginViewModel viewModel = Login_Arrange_MockViewModel();
        
        // Act
        IActionResult actual = await controller.PostLogin(viewModel);

        // Assert
        actual.AssertAsExactViewModel(viewModel, "Login");
    }

    [Test]
    public async Task PostLogin_WillCheckLoginValid_AndReturnToIndexWithSuccessMessageAndCookie()
    {
        // Arrange
        LoginViewModel viewModel = Login_Arrange_MockViewModel();
        Mock<IUserRepository> mockRepo = Arrange_MockRepo();
        Arrange_MockRepo_CheckLoginValid(mockRepo, viewModel);
        
        Mock<INotyfService> mockNotyf = Arrange_MockNotyf();

        // HttpContext.SignInAsync actually calls some extension methods,
        // that calls IServiceProvider to get IAuthenticationService and call the actual SignInAsync there.
        // Mocking extension methods directly seems unsupported,
        // thus we can't mock HttpContext directly.
        
        // Thus, we need to mock:
        
        // 1. IServiceProvider (injects into HttpContext through ControllerContext)
        // 2. IAuthenticationService (injects into IServiceProvider)
        
        // Besides these, due to MVC needs,
        // we also need to mock:
        
        // 3. IUrlHelperFactory (injects into IServiceProvider)
        
        Mock<IAuthenticationService> mockAuthService = new();
        PostLogin_Arrange_MockAuthServiceCookieSignIn(mockAuthService);

        (UserController controller, Mock<IServiceProvider> mockServiceProvider) 
            = Arrange_ControllerWithMockedServices(mockRepo, mockNotyf);

        Arrange_ProvideMockAuthService(mockServiceProvider, mockAuthService);
        Arrange_ProvideMockUrlHelperFactory(mockServiceProvider);

        // Act
        IActionResult actual = await controller.PostLogin(viewModel);

        // Assert
        mockRepo.VerifyAll();
        mockAuthService.VerifyAll();
        Assert_Notyf_SuccessShown(mockNotyf);
        actual.AssertAsRedirectToActionResult("Index", "Home");
    }
    
    [Test]
    public async Task PostLogin_WillCheckLoginInvalid_AndReturnSameViewWithErrorMessageAndCookieUntouched()
    {
        // Arrange
        LoginViewModel viewModel = Login_Arrange_MockViewModel();
        Mock<IUserRepository> mockRepo = Arrange_MockRepo();
        Arrange_MockRepo_CheckLoginInvalid(mockRepo, viewModel);
        
        Mock<INotyfService> mockNotyf = Arrange_MockNotyf();
        
        // For why ServiceProvider is being mocked,
        // Check the other PostLogin test method.
        (UserController controller, Mock<IServiceProvider> mockServiceProvider) 
            = Arrange_ControllerWithMockedServices(mockRepo, mockNotyf);

        // MVC needs this.
        PostLogin_Arrange_ProvideTempDataDictionaryFactory(mockServiceProvider);

        // Act
        IActionResult actual = await controller.PostLogin(viewModel);

        // Assert
        mockRepo.VerifyAll();
        mockServiceProvider.Verify(m => m.GetService(typeof(IAuthenticationService)), Times.Never);
        Assert_Notyf_ErrorShown(mockNotyf);
        actual.AssertAsExactViewModel(viewModel, "Login");
    }

    [Test]
    public void GetRegister_WillReturnRegisterView()
    {
        // Arrange
        UserController controller = Arrange_Controller(Arrange_MockRepo(), Arrange_MockNotyf());

        // Act
        IActionResult actual = controller.GetRegister();

        // Assert
        actual.AssertAsViewResult("Register");
    }
    
    [Test]
    public void GetLogin_WillReturnRegisterView()
    {
        // Arrange
        UserController controller = Arrange_Controller(Arrange_MockRepo(), Arrange_MockNotyf());

        // Act
        IActionResult actual = controller.GetLogin();

        // Assert
        actual.AssertAsViewResult("Login");
    }
    
    [Test]
    public async Task Logout_WillSignOut_AndRedirectToGetLoginWithMessage()
    {
        // Arrange
        Mock<INotyfService> mockNotyf = Arrange_MockNotyf();

        // For why ServiceProvider is being mocked,
        // Check the other PostLogin test method.
        Mock<IAuthenticationService> mockAuthService = new();
        Logout_Arrange_MockAuthServiceCookieSignOut(mockAuthService);

        (UserController controller, Mock<IServiceProvider> mockServiceProvider) 
            = Arrange_ControllerWithMockedServices(Arrange_MockRepo(), mockNotyf);

        Arrange_ProvideMockAuthService(mockServiceProvider, mockAuthService);
        Arrange_ProvideMockUrlHelperFactory(mockServiceProvider);

        // Act
        IActionResult actual = await controller.Logout();

        // Assert
        mockAuthService.VerifyAll();
        Assert_Notyf_InformationShown(mockNotyf);
        actual.AssertAsRedirectToActionResult("GetLogin");
    }

    [Test]
    public void HandleLoginError_WillShowErrorMessage_AndRedirectToGetLogin()
    {
        // Arrange
        Mock<INotyfService> mockNotyf = Arrange_MockNotyf();
        UserController controller = Arrange_Controller(Arrange_MockRepo(), mockNotyf);

        // Act
        IActionResult actual = controller.HandleLoginError();

        // Assert
        Assert_Notyf_ErrorShown(mockNotyf);
        actual.AssertAsRedirectToActionResult("GetLogin");
    }

    private static void PostLogin_Arrange_MockAuthServiceCookieSignIn(Mock<IAuthenticationService> mockAuthService)
    {
        mockAuthService.Setup(m => m.SignInAsync(It.IsAny<HttpContext>()
                , CookieAuthenticationDefaults.AuthenticationScheme
                , It.IsAny<ClaimsPrincipal>()
                , It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);
    }
    
    private static void Logout_Arrange_MockAuthServiceCookieSignOut(Mock<IAuthenticationService> mockAuthService)
    {
        mockAuthService.Setup(m => m.SignOutAsync(It.IsAny<HttpContext>()
                , CookieAuthenticationDefaults.AuthenticationScheme, default))
            .Returns(Task.CompletedTask);
    }


    private static void Arrange_ProvideMockAuthService(Mock<IServiceProvider> mockServiceProvider, Mock<IAuthenticationService> mockAuthService)
    {
        mockServiceProvider.Setup(m => m.GetService(typeof(IAuthenticationService)))
            .Returns(mockAuthService.Object);
    }

    private static void Arrange_ProvideMockUrlHelperFactory(Mock<IServiceProvider> mockServiceProvider)
    {
        // Simply for MVC. Seems like there's no further setup needed. 
        mockServiceProvider.Setup(m => m.GetService(typeof(IUrlHelperFactory)))
            .Returns(Mock.Of<IUrlHelperFactory>());
    }
    
    private static void PostLogin_Arrange_ProvideTempDataDictionaryFactory(Mock<IServiceProvider> mockServiceProvider)
    {
        // Simply for MVC. Seems like there's no further setup needed. 
        mockServiceProvider.Setup(m => m.GetService(typeof(ITempDataDictionaryFactory)))
            .Returns(Mock.Of<ITempDataDictionaryFactory>());
    }

    private static void Arrange_MockRepo_CheckLoginValid(Mock<IUserRepository> mockRepo, LoginViewModel viewModel)
    {
        mockRepo.Setup(m => m.CheckLoginValid(UserLoginComparer(viewModel)))
            .ReturnsAsync((true, new()
            {
                Username = viewModel.Username!,
                Password = viewModel.Password!
            }));
    }
    
    private static void Arrange_MockRepo_CheckLoginInvalid(Mock<IUserRepository> mockRepo, LoginViewModel viewModel)
    {
        mockRepo.Setup(m => m.CheckLoginValid(UserLoginComparer(viewModel)))
            .ReturnsAsync((false, null));
    }

    private static void Arrange_MockRepo_Create(Mock<IUserRepository> mockRepo, RegisterViewModel viewModel)
    {
        mockRepo.Setup(m => m.Create(UserRegisterComparer(viewModel)))
            .ReturnsAsync(new User
            {
                Username = viewModel.Username!,
                Password = viewModel.Password!
            });
    }
    
    private static void Arrange_MockRepo_CreateWillThrow(Mock<IUserRepository> mockRepo, RegisterViewModel viewModel)
    {
        mockRepo.Setup(m => m.Create(UserRegisterComparer(viewModel)))
            .Throws<ArgumentException>();
    }
    
    private static void Assert_Notyf_SuccessShown(Mock<INotyfService> mockNotyf)
    {
        mockNotyf.Verify(m 
            => m.Success(It.IsAny<string>(), default), Times.AtLeastOnce);
    }
    
    private static void Assert_Notyf_ErrorShown(Mock<INotyfService> mockNotyf)
    {
        mockNotyf.Verify(m 
            => m.Error(It.IsAny<string>(), default), Times.AtLeastOnce);
    }
    
    private static void Assert_Notyf_InformationShown(Mock<INotyfService> mockNotyf)
    {
        mockNotyf.Verify(m 
            => m.Information(It.IsAny<string>(), default), Times.AtLeastOnce);
    }

    private static void Assert_PostRepo_CreatedOnce(Mock<IUserRepository> mockRepo, RegisterViewModel viewModel)
    {
        mockRepo.Verify(m => m.Create(UserRegisterComparer(viewModel)), Times.Once);
    }
    
    private static User UserRegisterComparer(RegisterViewModel viewModel) 
        => It.Is<User>(u 
            => u.Username == viewModel.Username 
               && u.Password == viewModel.Password);
    
    private static User UserLoginComparer(LoginViewModel viewModel) 
        => It.Is<User>(u 
            => u.Username == viewModel.Username 
               && u.Password == viewModel.Password);

    private static UserController Arrange_Controller(IMock<IUserRepository> mockRepo, IMock<INotyfService> mockNotyf)
    {
        UserController controller = new(mockRepo.Object, mockNotyf.Object);
        return controller;
    }
    
    private static (UserController, Mock<IServiceProvider>) Arrange_ControllerWithMockedServices(IMock<IUserRepository> mockRepo, IMock<INotyfService> mockNotyf)
    {
        Mock<IServiceProvider> mockService = new();
        UserController controller = new(mockRepo.Object, mockNotyf.Object)
        {
            ControllerContext = new()
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = mockService.Object
                }
            }
        };
        return (controller, mockService);
    }
    
    private static Mock<INotyfService> Arrange_MockNotyf()
    {
        return new Mock<INotyfService>();
    }

    private static Mock<IUserRepository> Arrange_MockRepo()
    {
        return new Mock<IUserRepository>();
    }

    private static RegisterViewModel Register_Arrange_MockViewModel()
    {
        RegisterViewModel viewModel = new()
        {
            Username = MockUsername,
            Password = MockPassword
        };
        return viewModel;
    }
    
    private static LoginViewModel Login_Arrange_MockViewModel()
    { 
        LoginViewModel viewModel = new()
        {
            Username = MockUsername,
            Password = MockPassword
        };
        return viewModel;
    }
}