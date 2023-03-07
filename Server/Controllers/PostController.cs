using System.Globalization;
using System.Web;
using AspNetCoreHero.ToastNotification.Abstractions;
using DemoForum.Enums;
using DemoForum.Models;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using DemoForum.Utils;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPost("Edit")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostEditResult(EditorViewModel editor)
    {
        if (!ModelState.IsValid)
            return EditorView(editor);
        
        try
        {
            Post entity = new()
            {
                Title = editor.PostTitle!,
                Content = editor.PostContent!,
                AuthorId = HttpContext.GetUserIdFromClaimsInt()
            };
            
            switch (editor.PostMode)
            {
                case PostMode.Edit:
                    entity = await _postRepository.Update(editor.EntityId, entity);
                    break;
                case PostMode.New:
                default:
                    entity = await _postRepository.Create(entity);
                    break;
            }

            _notyfService.Success($"{editor.PostMode.GetChinese()}成功！");
            
            return RedirectToAction("Read", new {id = entity.Id});
        }
        catch (Exception e)
        {
            _logger.LogError($"GetEditEditor failed, mode {editor.PostMode}, {editor}");
            _logger.LogError($"Cookie sidString: {HttpContext.GetUserIdFromClaims()}");
            _logger.LogError(e.ToString());
            _notyfService.Error($"{editor.PostMode.GetChinese()}失敗，可能是網站忙碌中，請稍後再試 ...");
            _notyfService.Error(HttpUtility.JavaScriptStringEncode(e.Message));
            
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(PostViewModel postViewModel)
    {
        Post? post = await _postRepository.Read(postViewModel.Id ?? throw new NullReferenceException("Post id is null!"));
        if (post == null)
            throw new NullReferenceException("欲修改的文章查無資料，可能已被刪除 ...");
        
        EditorViewModel editViewModel = new()
        {
            EntityId = post.Id,
            PostTitle = post.Title,
            PostContent = post.Content,
            PostMode = PostMode.Edit
        };

        IActionResult result = EditorView(editViewModel);
        return result;
    }

    [HttpGet("New")]
    [Authorize]
    public IActionResult GetNewEditor()
    {
        return EditorView(PostMode.New);
    }

    [HttpPost("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePostConfirmation(PostViewModel post)
    {
        if (post.Id == null)
        {
            _notyfService.Error("要刪除的文章似乎已被刪除 ...");
            _logger.LogError("Tried to get deletePostConfirmation yet post.Id is null!");
            return RedirectToAction("Index", "Home");
        }
        
        DeletePostConfirmationViewModel viewModel = new()
        {
            PostId = post.Id
        };
        return View(viewModel);
    }

    private IActionResult EditorView(PostMode mode)
    {
        EditorViewModel editorViewModel = new()
        {
            PostMode = mode
        };

        return EditorView(editorViewModel);
    }
    
    private IActionResult EditorView(EditorViewModel viewModel)
    {
        return View("Editor", viewModel);
    }

    [HttpGet("Read/{id}")]
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
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedTime = post.CreatedTime.ToString(CultureInfo.CurrentCulture),
            AuthorName = post.Author.Username,
            UpdatedTime = post.UpdatedTime?.ToString(CultureInfo.CurrentCulture),
            CommentViews = post.Comments.Select(c => new CommentViewModel
            {
                CommentMode = CommentModeHelper.ByDbType(c.Type),
                Username = c.Author.Username,
                Content = c.Content,
                CreatedTime = c.CreatedTime.ToString(CultureInfo.CurrentCulture)
            })
                .OrderBy(cv => cv.CreatedTime),
            CommentScore = post.CommentScore
        };

        ViewResult view = View(postViewModel);
        view.ViewData["PushChinese"] = CommentMode.Push.GetChinese();
        view.ViewData["BooChinese"] = CommentMode.Boo.GetChinese();
        view.ViewData["NaturalChinese"] = CommentMode.Natural.GetChinese();
        
        return view;
    }
    
    public async Task<IActionResult> ReadFromDeletePostConfirmation(DeletePostConfirmationViewModel model)
    {
        if (model.PostId == null)
        {
            _notyfService.Error("就在你猶豫的時候，文章已經被刪除的樣子 ...");
            return RedirectToAction("Index", "Home");
        }
        
        IActionResult result 
            = await Read((int)model.PostId);
        ViewResult viewResult = (ViewResult)result;
        
        return View("Read", viewResult.Model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(DeletePostConfirmationViewModel model)
    {
        if (model.PostId != null)
        {
            try
            {
                await _postRepository.Delete((int)model.PostId);
            }
            catch (NullReferenceException nre)
            {
                _logger.LogInformation(nre.Message);
            }
        }
        else
        {
            _logger.LogWarning("Delete received a DeletePostConfirmation with null PostId.");
        }

        _notyfService.Success("文章刪除成功。");
        return RedirectToAction("Index", "Home");
    }
}