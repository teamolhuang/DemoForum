using System.Transactions;
using AspNetCoreHero.ToastNotification.Abstractions;
using DemoForum.Enums;
using DemoForum.Models;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using DemoForum.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoForum.Controllers;

public class CommentController : Controller
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotyfService _notyf;

    public CommentController(ICommentRepository commentRepository, IPostRepository postRepository, IUserRepository userRepository, INotyfService notyf)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _notyf = notyf;
    }
    
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Push(CommentInputViewModel model)
    {
        return await PostComment(model, CommentMode.Push);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Boo(CommentInputViewModel model)
    {
        return await PostComment(model, CommentMode.Boo);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Natural(CommentInputViewModel model)
    {
        return await PostComment(model, CommentMode.Natural);
    }

    private async Task<IActionResult> PostComment(CommentInputViewModel viewModel, CommentMode mode)
    {
        Post? post = await _postRepository
            .Read(viewModel.PostId
                  ?? throw new NullReferenceException("PostComment got a null postId from view"));

        if (post == null)
        {
            _notyf.Information($"糟糕！你剛要{mode.GetChinese()}的文章已經刪除了！");
            return RedirectToAction("Index", "Home");
        }

        User? user = await _userRepository.Read(HttpContext.GetUserIdFromClaimsInt());

        if (user == null)
        {
            // Claims 或登入資訊有誤，要求重新登入
            _notyf.Error("帳號資訊有誤或登入逾時，請重新登入。");
            return RedirectToAction("Logout", "User");
        }

        // 綁定在同一個 TransactionScope, 保證推文紀錄的一致性
        using (TransactionScope tx = new(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {

                // 更新 CommentScore
                post.CommentScore += mode.GetCommentScore();
                await _postRepository.Update(post.Id, post);

                // 寫入 Comment
                Comment comment = new()
                {
                    AuthorId = user.Id,
                    Author = user,
                    PostId = post.Id,
                    Post = post,
                    Content = viewModel.CommentContent!,
                    CreatedTime = DateTime.Now,
                    Type = mode.GetDbEnum()
                };

                await _commentRepository.Create(comment);
                tx.Complete();

            }
            catch (Exception e)
            {
                _notyf.Error("網站忙碌中，請稍後重試 ...");
                return RedirectToAction("Read", "Post", new {Id = viewModel.PostId});
            }
        }

        _notyf.Success("推文成功！");
        return RedirectToAction("Read", "Post", new {Id = viewModel.PostId});
    }
}