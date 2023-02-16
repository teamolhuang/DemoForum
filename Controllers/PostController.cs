using DemoForum.Models;
using Microsoft.AspNetCore.Mvc;

namespace DemoForum.Controllers;

public class PostController : Controller
{
    [HttpPost]
    public IActionResult EditPost(EditPostViewModel editPost)
    {
        if (!ModelState.IsValid) 
            return View(editPost);

        return Ok();
    }

    [HttpGet]
    public IActionResult EditPost()
    {
        return View();
    }
}