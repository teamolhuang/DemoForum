using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DemoForumTests.Controllers;

public static class ExtensionMethods
{
    public static void AddModelMockError(this ModelStateDictionary dict)
    {
        dict.AddModelError("test", "test");
    }
}