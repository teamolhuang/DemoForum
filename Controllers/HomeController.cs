using System.Diagnostics;
using System.Globalization;
using DemoForum.Models;
using DemoForum.Repositories;
using DemoForum.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DemoForum.Controllers;

public class HomeController : Controller
{
    private readonly IPostRepository _postRepository;

    public HomeController(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public IActionResult Index()
    {
        HomeViewModel viewModel = new()
        {
            Posts = _postRepository.ReadLatest(100)
                .Select(p => new PostPreviewViewModel
                {
                    Id = p.Id.ToString().PadLeft(7, '0'),
                    Title = p.Title.ShortenToPreviewDefault(),
                    Content = p.Content.ShortenToPreviewDefault(40),
                    CreatedTime = p.CreatedTime.ToString(CultureInfo.CurrentCulture)
                })
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}