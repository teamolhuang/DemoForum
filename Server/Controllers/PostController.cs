using System.Globalization;
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
    private readonly ILogger<PostController> _logger;
    private readonly INotyfService _notyfService;
    private readonly IPostRepository _postRepository;

    public PostController(IPostRepository postRepository, INotyfService notyfService, ILogger<PostController> logger)
    {
        _postRepository = postRepository;
        _notyfService = notyfService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> EditPost(EditPostViewModel editPost)
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
            switch (editPost.PostMode)
            {
                case PostMode.New:
                default:
                    await _postRepository.Create(entity);
                    break;
                case PostMode.Edit:
                    await _postRepository.Update(editPost.EntityId, entity);
                    break;
            }

            _notyfService.Success("發文成功！");
        }
        catch (Exception e)
        {
            _logger.LogError($"GetEditEditor failed, mode {editPost.PostMode}, {editPost}");
            _logger.LogError(e.ToString());
            _notyfService.Error("發文失敗，請通知網站管理員 ...");
            _notyfService.Error(HttpUtility.JavaScriptStringEncode(e.Message));
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult GetEditEditor()
    {
        return EditorView(PostMode.Edit);
    }

    [HttpGet]
    public IActionResult GetNewEditor()
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

    [HttpGet]
    public async Task<IActionResult> Read(int id)
    {
        Post? post = await _postRepository.Read(id);

        if (post == null)
        {
            _notyfService.Error("點擊的文章似乎已被刪除 ...");
            return View();
        }
        
        PostViewModel postViewModel = new()
        {
            Title = post.Title,
            Content = post.Content,
            CreatedTime = post.CreatedTime.ToString(CultureInfo.CurrentCulture)
        };
        
        return View(postViewModel);
    }
}