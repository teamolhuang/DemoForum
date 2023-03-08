using System.Diagnostics;
using System.Globalization;
using AspNetCoreHero.ToastNotification.Abstractions;
using DemoForum.Models;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using DemoForum.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DemoForum.Controllers;

public class HomeController : Controller
{
    private readonly IPostRepository _postRepository;
    private INotyfService _notyf;

    public HomeController(IPostRepository postRepository, INotyfService notyf)
    {
        _postRepository = postRepository;
        _notyf = notyf;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        IEnumerable<Post> posts = await _postRepository.ReadLatest(100);
        HomeViewModel viewModel = new()
        {
            Posts = posts.Select(p => new PostPreviewViewModel
                {
                    Id = p.Id.ToString().PadLeft(7, '0'),
                    Title = p.Title.ShortenToPreviewDefault(),
                    Content = p.Content.ShortenToPreviewDefault(40),
                    CreatedTime = p.CreatedTime.ToStringForView(),
                    AuthorName = p.Author.Username,
                    CommentScore = p.CommentScore
                })
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult RedirectToIndex()
    {
        _notyf.Error("處理失敗，可能是網站忙碌中 ...");
        return RedirectToAction("Index");
    }
}