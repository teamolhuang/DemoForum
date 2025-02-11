using System.Net;
using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using DemoForum.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add dbcontext
// ensure db subfolder exists. we want to put the db file there.

Directory.CreateDirectory("db");

builder.Services.AddDbContext<ForumContext>(options =>
{
    options.UseSqlite("Data Source=./db/database.db");
});

// Adds toast notification
builder.Services.AddNotyf(config =>
{
    config.Position = NotyfPosition.TopCenter;
    config.DurationInSeconds = 10;
    config.HasRippleEffect = true;
    config.IsDismissable = true;
});

// Scan and add services
builder.Services.AddLogging();
builder.Services.AddScoped<ForumContext, ForumContext>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

// Add cookie auth for sign-in
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = Consts.CookieKey;
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(Consts.LoginExpirationMinutes);
        options.SlidingExpiration = true;
        options.LoginPath = "/User/HandleLoginError";
    });

builder.Services.AddAuthorization();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = Consts.AntiForgeryCookie;
    options.HeaderName = Consts.AntiForgeryCookieHeader;
});

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
});

// Sustains cookie keys so that users don't need to re-login whenever container is down.
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/key/storage"));
}

WebApplication app = builder.Build();
        
// 先確保已建立並更新 db
await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
await using ForumContext db = scope.ServiceProvider.GetRequiredService<ForumContext>();
await db.Database.MigrateAsync();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseStatusCodePages(async context =>
{
    switch (context.HttpContext.Response.StatusCode)
    {
        default:
            context.HttpContext.Response.Redirect(new PathString("/Home/RedirectToIndex"));
            break;
    }

    await Task.CompletedTask;
});

app.UseNotyf();

app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();