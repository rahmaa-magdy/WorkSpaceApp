using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;
using WorkSpaceApp.Services.Interfaces;
using WorkSpaceApp.ViewModels;
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
        private readonly IWorkspaceAuthorizationService _authorizationService;
        private readonly IWorkTaskRepository _taskRepository;

        public ProjectController(
            IProjectRepository projectRepository,
            IWorkspaceRepository workspaceRepository,
            UserManager<ApplicationUser> userManager,
            IWorkTaskRepository taskRepository,
            IWorkspaceAuthorizationService authorizationService)
        {
            _projectRepository = projectRepository;
            _workspaceRepository = workspaceRepository;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _taskRepository = taskRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int workspaceId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);

            if (workspace == null)
                return NotFound();

            if (!await _authorizationService.CanCreateAsync(workspaceId, userId))
                return Forbid();

            ViewBag.WorkspaceName = workspace.Name;

            return View(new ProjectCreateViewModel
            {
                WorkspaceId = workspaceId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var workspace = await _workspaceRepository.GetByIdAsync(model.WorkspaceId);

            if (workspace == null)
                return NotFound();

            if (!await _authorizationService.CanCreateAsync(
                    model.WorkspaceId, userId))
                return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.WorkspaceName = workspace.Name;
                return View(model);
            }

            var project = new Project
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim(),
                Deadline = model.Deadline,
                Status = model.Status,
                WorkspaceId = model.WorkspaceId
            };

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveAsync();

            TempData["Success"] = "Project created successfully.";

            return RedirectToAction(
                "Details",
                "Workspace",
                new { id = model.WorkspaceId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var project = await _projectRepository.GetDetailsAsync(id);

            if (project == null)
                return NotFound();

            if (!await _authorizationService.IsMemberAsync(
                    project.WorkspaceId, userId))
                return Forbid();

            return View(project);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
                return NotFound();

            if (!await _authorizationService.CanCreateAsync(
                    project.WorkspaceId, userId))
                return Forbid();

            var model = new ProjectEditViewModel
            {
                Id = project.Id,
                WorkspaceId = project.WorkspaceId,
                Name = project.Name,
                Description = project.Description,
                Deadline = project.Deadline,
                Status = project.Status
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectEditViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var project = await _projectRepository.GetByIdAsync(model.Id);

            if (project == null)
                return NotFound();

            if (!await _authorizationService.CanCreateAsync(
                    project.WorkspaceId, userId))
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            project.Name = model.Name.Trim();
            project.Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim();
            project.Deadline = model.Deadline;
            project.Status = model.Status;

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveAsync();

            TempData["Success"] = "Project updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = project.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
                return NotFound();

            if (!await _authorizationService.CanDeleteAsync(
                    project.WorkspaceId, userId))
                return Forbid();

            var workspaceId = project.WorkspaceId;

            await _projectRepository.DeleteAsync(project);
            await _projectRepository.SaveAsync();

            TempData["Success"] = "Project deleted successfully.";

            return RedirectToAction(
                "Details",
                "Workspace",
                new { id = workspaceId });
        }

        [HttpGet]
        public async Task<IActionResult> Tasks(
    int id,
    string? search,
    WorkSpaceApp.Models.TaskStatus? status,
    TaskPriority? priority,
    int page = 1)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var project =
                await _projectRepository.GetDetailsAsync(id);

            if (project == null)
                return NotFound();

            if (!await _authorizationService.IsMemberAsync(
                    project.WorkspaceId,
                    userId))
                return Forbid();

            if (page < 1)
                page = 1;

            const int pageSize = 10;

            var result =
                await _taskRepository.SearchProjectTasksAsync(
                    id,
                    search,
                    status,
                    priority,
                    page,
                    pageSize);

            var model = new ProjectTasksViewModel
            {
                Project = project,
                Tasks = result.Tasks,
                Search = search,
                Status = status,
                Priority = priority,
                Page = page,
                PageSize = pageSize,
                TotalItems = result.TotalItems
            };

            return View(model);
        }
    }
}