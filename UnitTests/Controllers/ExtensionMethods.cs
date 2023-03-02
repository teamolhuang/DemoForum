using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;

namespace DemoForumTests.Controllers;

public static class ExtensionMethods
{
    /// <summary>
    /// Adds a mock error to ModelStateDictionary for ModelState tests.
    /// </summary>
    /// <param name="dict">ModelStateDictionary</param>
    public static void AddModelMockError(this ModelStateDictionary dict)
    {
        dict.AddModelError("test", "test");
    }

    /// <summary>
    /// Asserts an ActionResult as a ViewResult.
    /// </summary>
    /// <param name="actionResult">any ActionResult</param>
    /// <param name="viewName">View name in string (Optional)</param>
    /// <returns>ViewResult after asserted correctly</returns>
    public static ViewResult AssertAsViewResult(this IActionResult actionResult
        , string? viewName = null)
    {
        viewName ??= It.IsAny<string>();

        Assert.IsNotNull(actionResult);
        Assert.IsAssignableFrom<ViewResult>(actionResult);
        ViewResult viewResult = (ViewResult)actionResult;
        Assert.AreEqual(viewName, viewResult.ViewName);

        return viewResult;
    }

    /// <summary>
    /// Asserts an ActionResult as a RedirectToActionResult.
    /// </summary>
    /// <param name="actionResult">any ActionResult</param>
    /// <param name="actionName">Action name in string (Optional)</param>
    /// <param name="controllerName">Controller name in string (Optional)</param>
    /// <returns>RedirectToActionResult after asserted correctly</returns>
    public static RedirectToActionResult AssertAsRedirectToActionResult(this IActionResult actionResult
        , string? actionName = null
        , string? controllerName = null)
    {
        actionName ??= It.IsAny<string>();
        controllerName ??= It.IsAny<string>();
        
        Assert.IsNotNull(actionResult);
        Assert.IsAssignableFrom<RedirectToActionResult>(actionResult);
        RedirectToActionResult redirectResult = (RedirectToActionResult)actionResult;
        Assert.AreEqual(actionName, redirectResult.ActionName);
        Assert.AreEqual(controllerName, redirectResult.ControllerName);

        return redirectResult;
    }

    /// <summary>
    /// Asserts that an ActionResult is a ViewResult,<br/>
    /// and it contains a viewModel exactly same as input view model.
    /// </summary>
    /// <param name="actionResult">any ActionResult</param>
    /// <param name="viewModel">View model</param>
    /// <param name="viewName">View name (Optional)</param>
    /// <typeparam name="T">View model type</typeparam>
    public static T? AssertAsExactViewModel<T>(this IActionResult actionResult
        , T viewModel, string? viewName = null)
    {
        return actionResult.AssertAsViewResult(viewName).AssertAsExactViewModel(viewModel);
    }

    /// <summary>
    /// Asserts that a ViewResult contains a viewModel exactly same as input view model.
    /// </summary>
    /// <param name="viewResult">any ViewResult</param>
    /// <param name="viewModel">View model</param>
    /// <typeparam name="T">View model type</typeparam>
    public static T? AssertAsExactViewModel<T>(this ViewResult viewResult
        , T viewModel)
    {
        Assert.IsAssignableFrom<T>(viewResult.Model);
        Assert.AreEqual(viewModel, viewResult.Model);

        return (T?)viewResult.Model;
    }

    /// <summary>
    /// Asserts that an ActionResult is actually a T-type view model.
    /// </summary>
    /// <param name="actionResult">any ActionResult</param>
    /// <typeparam name="T">View Model type</typeparam>
    /// <returns>view model</returns>
    public static T AssertAsViewModel<T>(this IActionResult actionResult)
    {
        ViewResult viewResult = actionResult.AssertAsViewResult();
        Assert.IsNotNull(viewResult.Model);
        Assert.IsAssignableFrom<T>(viewResult.Model);
        return (T)viewResult.Model!;
    }
}