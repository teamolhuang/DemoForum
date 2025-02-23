﻿using System.Security.Claims;
using AspNetCoreHero.ToastNotification.Abstractions;
using DemoForum.Models;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using DemoForum.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoForum.Controllers;

public class UserController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly INotyfService _notyf;

    public UserController(IUserRepository userRepository, INotyfService notyf)
    {
        _userRepository = userRepository;
        _notyf = notyf;
    }
    
    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostRegister(RegisterViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View("Register", viewModel);
        
        User user = new()
        {
            Username = viewModel.Username!,
            Password = viewModel.Password!
        };

        try
        {
            await _userRepository.Create(user);
        }
        catch (ArgumentException)
        {
            _notyf.Error("此帳號已被註冊！");
            return View("Register", viewModel);
        }

        _notyf.Success("註冊成功！");
        return RedirectToAction("GetLogin");
    }
    
    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostLogin(LoginViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View("Login", viewModel);
        
        // Check if login is valid through db
        (bool isValid, User? entity) = await _userRepository.CheckLoginValid(new()
        {
            Username = viewModel.Username!,
            Password = viewModel.Password!
        });

        if (!isValid || entity == null)
        {
            _notyf.Error("帳號不存在或登入帳密有誤！請檢查登入資訊。");
            return View("Login", viewModel);
        }
        
        await LoginCookie(entity);
        _notyf.Success("登入成功！");
        return RedirectToAction("Index", "Home");
    }

    private async Task LoginCookie(User entity)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Sid, entity.Id.ToString()),
            new(ClaimTypes.NameIdentifier, entity.Username)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(Consts.LoginExpirationMinutes),
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme
            , new ClaimsPrincipal(claimsIdentity)
            , authProperties);
    }

    [HttpGet("Register")]
    public IActionResult GetRegister()
    {
        return View("Register");
    }

    [HttpGet("Login")]
    public IActionResult GetLogin()
    {
        return View("Login");
    }

    [AcceptVerbs("GET", "POST")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _notyf.Information("已登出，請重新登入。");
        return RedirectToAction("GetLogin");
    }

    public IActionResult HandleLoginError()
    {
        _notyf.Error("未登入或登入逾時，請重新登入 ...");
        return RedirectToAction("GetLogin");
    }
}