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
        private readonly IWorkspaceMemberRepository _memberRepository;

        public WorkspaceController(
    IWorkspaceRepository workspaceRepository,
    IWorkspaceMemberRepository memberRepository,
    UserManager<ApplicationUser> userManager)
        {
            _workspaceRepository = workspaceRepository;
            _memberRepository = memberRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var workspaces =
                await _workspaceRepository
                    .GetUserWorkspacesAsync(userId!);

            return View(workspaces);
        }

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

            await _workspaceRepository.AddAsync(workspace);

            await _workspaceRepository.SaveAsync();

            var userId = _userManager.GetUserId(User);

            var membership = new WorkspaceMember
            {
                WorkspaceId = workspace.Id,
                UserId = userId!,
                Role = WorkspaceRole.Owner
            };

            await _memberRepository.AddAsync(membership);

            await _memberRepository.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var workspace =
                await _workspaceRepository.GetDetailsAsync(id);

            if (workspace == null)
            {
                return NotFound();
            }

            return View(workspace);
        }
    }
}