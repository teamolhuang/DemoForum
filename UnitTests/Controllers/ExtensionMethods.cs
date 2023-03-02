using Microsoft.AspNetCore.Mvc.ModelBinding;

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
}