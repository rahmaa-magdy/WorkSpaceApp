using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;

namespace WorkSpaceApp.Controllers
{
    [Authorize]
    public class WorkspaceController : Controller
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkspaceController(
            IWorkspaceRepository workspaceRepository,
            UserManager<ApplicationUser> userManager)
        {
            _workspaceRepository = workspaceRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var workspaces =
                await _workspaceRepository
                    .GetUserWorkspacesAsync(userId);

            return View(workspaces);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Workspace workspace)
        {
            if (!ModelState.IsValid)
            {
                return View(workspace);
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            await _workspaceRepository.CreateAsync(
                workspace,
                userId);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var workspace =
                await _workspaceRepository.GetDetailsAsync(id);

            if (workspace == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var isMember = workspace.Members
                .Any(m => m.UserId == userId);

            if (!isMember)
            {
                return Forbid();
            }

            return View(workspace);
        }
    }
}