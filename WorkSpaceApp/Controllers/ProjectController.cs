using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;
using WorkSpaceApp.ViewModels;

namespace WorkSpaceApp.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectController(
            IProjectRepository projectRepository,
            IWorkspaceRepository workspaceRepository,
            UserManager<ApplicationUser> userManager)
        {
            _projectRepository = projectRepository;
            _workspaceRepository = workspaceRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(
            int workspaceId)
        {
            var workspace =
                await _workspaceRepository
                    .GetDetailsAsync(workspaceId);

            if (workspace == null)
            {
                return NotFound();
            }

            if (!IsMember(workspace))
            {
                return Forbid();
            }

            var model = new ProjectCreateViewModel
            {
                WorkspaceId = workspaceId
            };

            ViewBag.WorkspaceName = workspace.Name;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ProjectCreateViewModel model)
        {
            var workspace =
                await _workspaceRepository
                    .GetDetailsAsync(model.WorkspaceId);

            if (workspace == null)
            {
                return NotFound();
            }

            if (!IsMember(workspace))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.WorkspaceName = workspace.Name;
                return View(model);
            }

            var project = new Project
            {
                Name = model.Name,
                Description = model.Description,
                Deadline = model.Deadline,
                Status = model.Status,
                WorkspaceId = model.WorkspaceId
            };

            await _projectRepository.AddAsync(project);

            await _projectRepository.SaveAsync();

            return RedirectToAction(
                "Details",
                "Workspace",
                new { id = model.WorkspaceId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var project =
                await _projectRepository
                    .GetDetailsAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            if (!IsMember(project.Workspace))
            {
                return Forbid();
            }

            return View(project);
        }

        private bool IsMember(Workspace workspace)
        {
            var userId = _userManager.GetUserId(User);

            return workspace.Members
                .Any(m => m.UserId == userId);
        }
    }
}