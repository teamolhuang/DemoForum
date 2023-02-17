using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using DemoForum.Models.Entities;
using DemoForum.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add dbcontext
// This also reads connection strings from dotnet user-secret automatically,
// thus no need for actually configuring it into the project
// builder.Services.AddDbContext<ForumContext>();

// Adds toast notification
builder.Services.AddNotyf(config =>
{
    config.Position = NotyfPosition.TopRight;
    config.DurationInSeconds = 10;
    config.HasRippleEffect = true;
    config.IsDismissable = true;
});

// Scan and add services
builder.Services.AddLogging();
builder.Services.AddScoped<ForumContext, ForumContext>();
builder.Services.AddScoped<IPostRepository, PostRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseNotyf();

app.Run();