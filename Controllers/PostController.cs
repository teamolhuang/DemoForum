using DemoForum.Enums;
using DemoForum.Models;
using Microsoft.AspNetCore.Mvc;

namespace DemoForum.Controllers;

public class PostController : Controller
{
    [HttpPost]
    public IActionResult EditPost(EditPostViewModel editPost)
    {
        return !ModelState.IsValid ? EditorView(editPost) : Ok();
    }

    [HttpGet]
    public IActionResult EditPost()
    {
        return EditorView(PostMode.Edit);
    }

    [HttpGet]
    public IActionResult NewPost()
    {
        return EditorView(PostMode.New);
    }

    private IActionResult EditorView(PostMode mode)
    {
        EditPostViewModel editPostViewModel = new()
        {
            PostMode = mode
        };

        return EditorView(editPostViewModel);
    }
    
    private IActionResult EditorView(EditPostViewModel viewModel)
    {
        return View("EditPost", viewModel);
    }
}