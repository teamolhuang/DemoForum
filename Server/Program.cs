using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using DemoForum.Models.Entities;
using DemoForum.Repositories;
using DemoForum.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add dbcontext
// This also reads connection strings from dotnet user-secret automatically,
// thus no need for actually configuring it into the project
builder.Services.AddDbContext<ForumContext>();

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

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

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