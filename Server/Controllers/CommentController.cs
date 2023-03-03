using AspNetCoreHero.ToastNotification.Abstractions;
using DemoForum.Enums;
using DemoForum.Models;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DemoForum.Controllers;

public class CommentController : Controller
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly INotyfService _notyf;

    public CommentController(ICommentRepository commentRepository, IPostRepository postRepository, INotyfService notyf)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _notyf = notyf;
    }
    
    [HttpPost]
    public async Task<IActionResult> Push(PostViewModel model)
    {
        return await PostComment(model, CommentMode.Push);
    }

    [HttpPost]
    public async Task<IActionResult> Boo(PostViewModel model)
    {
        return await PostComment(model, CommentMode.Boo);
    }

    [HttpPost]
    public async Task<IActionResult> Natural(PostViewModel model)
    {
        return await PostComment(model, CommentMode.Natural);
    }

    private async Task<IActionResult> PostComment(PostViewModel viewModel, CommentMode mode)
    {
        Post? post = await _postRepository
            .Read(viewModel.Id 
                  ?? throw new NullReferenceException("PostComment got a null postId from view"));

        if (post == null)
        {
            _notyf.Information($"糟糕！你剛要{mode.GetChinese()}的文章已經刪除了！");
            return RedirectToAction("Index", "Home");
        }

        Comment comment = new()
        {
            AuthorId = post.AuthorId,
            PostId = post.Id,
            Content = viewModel.CommentContent!,
            CreatedTime = DateTime.Now,
            Type = mode.GetDbEnum(),
            Author = post.Author,
            Post = post
        };

        await _commentRepository.Create(comment);

        _notyf.Success("推文成功！");
        return RedirectToAction("Read", "Post", new {viewModel.Id});
    }
}