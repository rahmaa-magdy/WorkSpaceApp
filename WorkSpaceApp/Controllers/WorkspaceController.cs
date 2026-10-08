using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;
using WorkSpaceApp.Services.Interfaces;
using WorkSpaceApp.ViewModels;

namespace WorkSpaceApp.Controllers
{
    [Authorize]
    public class WorkspaceController : Controller
    {
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IWorkspaceMemberRepository _memberRepository;
        private readonly IWorkspaceAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkspaceController(
            IWorkspaceRepository workspaceRepository,
            IWorkspaceMemberRepository memberRepository,
            IWorkspaceAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager)
        {
            _workspaceRepository = workspaceRepository;
            _memberRepository = memberRepository;
            _authorizationService = authorizationService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var workspaces =
                await _workspaceRepository.GetUserWorkspacesAsync(userId);

            return View(workspaces);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Workspace workspace)
        {
            if (!ModelState.IsValid)
                return View(workspace);

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            workspace.Name = workspace.Name.Trim();

            if (!string.IsNullOrWhiteSpace(workspace.Description))
                workspace.Description = workspace.Description.Trim();

            await _workspaceRepository.CreateAsync(
                workspace,
                userId);

            TempData["Success"] =
                "Workspace created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            if (!await _authorizationService.IsMemberAsync(
                    id,
                    userId))
                return Forbid();

            var workspace =
                await _workspaceRepository.GetDetailsAsync(id);

            if (workspace == null)
                return NotFound();

            var role =
                await _authorizationService.GetUserRoleAsync(
                    id,
                    userId);

            ViewBag.CurrentRole = role;

            return View(workspace);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            if (!await _authorizationService.CanDeleteAsync(
                    id,
                    userId))
                return Forbid();

            var workspace =
                await _workspaceRepository.GetByIdAsync(id);

            if (workspace == null)
                return NotFound();

            var model = new WorkspaceEditViewModel
            {
                Id = workspace.Id,
                Name = workspace.Name,
                Description = workspace.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            WorkspaceEditViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            if (!await _authorizationService.CanDeleteAsync(
                    model.Id,
                    userId))
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            var workspace =
                await _workspaceRepository.GetByIdAsync(model.Id);

            if (workspace == null)
                return NotFound();

            workspace.Name = model.Name.Trim();

            workspace.Description =
                string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim();

            await _workspaceRepository.UpdateAsync(workspace);
            await _workspaceRepository.SaveAsync();

            TempData["Success"] =
                "Workspace updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = workspace.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            if (!await _authorizationService.CanDeleteAsync(
                    id,
                    userId))
                return Forbid();

            var workspace =
                await _workspaceRepository.GetByIdAsync(id);

            if (workspace == null)
                return NotFound();

            await _workspaceRepository.DeleteAsync(workspace);
            await _workspaceRepository.SaveAsync();

            TempData["Success"] =
                "Workspace deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}