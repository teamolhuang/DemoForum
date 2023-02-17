using System.Web;
using AspNetCoreHero.ToastNotification.Abstractions;
using DemoForum.Enums;
using DemoForum.Models;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DemoForum.Controllers;

public class PostController : Controller
{
    private readonly IPostRepository _postRepository;
    private readonly INotyfService _notyfService;
    private readonly ILogger<PostController> _logger;
    public PostController(IPostRepository postRepository, INotyfService notyfService, ILogger<PostController> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
        _notyfService = notyfService;
    }
    
    [HttpPost]
    public IActionResult EditPost(EditPostViewModel editPost)
    {
        if (!ModelState.IsValid)
            return EditorView(editPost);

        Post entity = new()
        {
            Title = editPost.PostTitle!,
            Content = editPost.PostContent!
        };

        try
        {
            if (editPost.PostMode == PostMode.New)
                _postRepository.Create(entity);
            if (editPost.PostMode == PostMode.Edit)
                _postRepository.Update(editPost.EntityId, entity);
            _notyfService.Success("發文成功！");
        } catch (Exception e)
        {
            _logger.LogError($"EditPost failed, mode {editPost.PostMode}, {editPost}");
            _logger.LogError(e.ToString());
            _notyfService.Error("發文失敗，請通知網站管理員 ...");
            _notyfService.Error(HttpUtility.JavaScriptStringEncode(e.Message));
        }
        
        return RedirectToAction("Index", "Home");
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