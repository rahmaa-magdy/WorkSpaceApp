using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Data;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories;
using WorkSpaceApp.Repositories.Interfaces;
using WorkSpaceApp.Services;
using WorkSpaceApp.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString(
                "DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            options.Password.RequireDigit = true;

            options.Password.RequiredLength = 6;

            options.Password.RequireNonAlphanumeric = false;

            options.Password.RequireUppercase = false;

            options.Password.RequireLowercase = false;

            options.User.RequireUniqueEmail = true;

            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(10);

            options.Lockout.MaxFailedAccessAttempts = 5;

            options.Lockout.AllowedForNewUsers = true;
        })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath = "/Account/Login";

        options.AccessDeniedPath =
            "/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

        options.SlidingExpiration = true;
    });

//repositories
builder.Services.AddScoped<
    IWorkspaceRepository,
    WorkspaceRepository>();

builder.Services.AddScoped<
    IWorkspaceMemberRepository,
    WorkspaceMemberRepository>();

builder.Services.AddScoped<
    IProjectRepository,
    ProjectRepository>();

builder.Services.AddScoped<
    IWorkTaskRepository,
    WorkTaskRepository>();

builder.Services.AddScoped<
    ICommentRepository,
    CommentRepository>();

//services
builder.Services.AddScoped<
    IWorkspaceAuthorizationService,
    WorkspaceAuthorizationService>();

var app = builder.Build();

//development seed
if (app.Environment.IsDevelopment())
{
    using var scope =
        app.Services.CreateScope();

    await DbInitializer.InitializeAsync(
        scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


//routing
app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");

app.Run();