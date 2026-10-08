using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Data;
using WorkSpaceApp.Models;
using WorkSpaceApp.ViewModels;

namespace WorkSpaceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Challenge();

            var workspaceIds = await _context.WorkspaceMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.WorkspaceId)
                .ToListAsync();

            var activeProjectCount = await _context.Projects
                .Where(p => workspaceIds.Contains(p.WorkspaceId))
                .CountAsync(p =>
                    p.Status == ProjectStatus.InProgress ||
                    p.Status == ProjectStatus.NotStarted);

            var openTaskCount = await _context.WorkTasks
                .Where(t =>
                    workspaceIds.Contains(t.Project.WorkspaceId))
                .CountAsync(t => t.Status != WorkSpaceApp.Models.TaskStatus.Done);

            var completedTaskCount = await _context.WorkTasks
                .Where(t =>
                    workspaceIds.Contains(t.Project.WorkspaceId))
                .CountAsync(t => t.Status == WorkSpaceApp.Models.TaskStatus.Done);

            var recentTasks = await _context.WorkTasks
                .Where(t =>
                    workspaceIds.Contains(t.Project.WorkspaceId))
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            var upcomingTasks = await _context.WorkTasks
                .Where(t =>
                    workspaceIds.Contains(t.Project.WorkspaceId) &&
                    t.Status != WorkSpaceApp.Models.TaskStatus.Done &&
                    t.DueDate.HasValue)
                .Include(t => t.Project)
                .OrderBy(t => t.DueDate)
                .Take(5)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                UserName = user.FullName,

                WorkspaceCount = workspaceIds.Count,

                ActiveProjectCount = activeProjectCount,

                OpenTaskCount = openTaskCount,

                CompletedTaskCount = completedTaskCount,

                RecentTasks = recentTasks,

                UpcomingTasks = upcomingTasks
            };

            return View(model);
        }
    }
}