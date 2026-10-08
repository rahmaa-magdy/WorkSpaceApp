using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Data;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;
using WorkSpaceApp.Services.Interfaces;
using WorkSpaceApp.ViewModels;

namespace WorkSpaceApp.Controllers
{
    [Authorize]
    public class WorkTaskController : Controller
    {
        private readonly IWorkTaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWorkspaceAuthorizationService _authorizationService;
        private readonly ApplicationDbContext _context;

        public WorkTaskController(
            IWorkTaskRepository taskRepository,
            IProjectRepository projectRepository,
            UserManager<ApplicationUser> userManager,
            IWorkspaceAuthorizationService authorizationService,
            ApplicationDbContext context)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int projectId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var project = await _projectRepository.GetDetailsAsync(projectId);

            if (project == null)
                return NotFound();

            if (!await _authorizationService.CanCreateAsync(
                    project.WorkspaceId, userId))
                return Forbid();

            await LoadMembers(project.WorkspaceId);

            ViewBag.ProjectName = project.Name;

            return View(new TaskCreateViewModel
            {
                ProjectId = projectId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskCreateViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var project = await _projectRepository.GetDetailsAsync(model.ProjectId);

            if (project == null)
                return NotFound();

            if (!await _authorizationService.CanCreateAsync(
                    project.WorkspaceId, userId))
                return Forbid();

            if (!string.IsNullOrEmpty(model.AssignedToUserId))
            {
                var isAssignedUserMember =
                    await _authorizationService.IsMemberAsync(
                        project.WorkspaceId,
                        model.AssignedToUserId);

                if (!isAssignedUserMember)
                {
                    ModelState.AddModelError(
                        nameof(model.AssignedToUserId),
                        "The selected user is not a member of this workspace.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadMembers(project.WorkspaceId);
                ViewBag.ProjectName = project.Name;
                return View(model);
            }

            var task = new WorkTask
            {
                Title = model.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim(),
                Priority = model.Priority,
                DueDate = model.DueDate,
                AssignedToUserId = model.AssignedToUserId,
                ProjectId = model.ProjectId,
                Status = WorkSpaceApp.Models.TaskStatus.ToDo
            };

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveAsync();

            TempData["Success"] = "Task created successfully.";

            return RedirectToAction(
                "Details",
                "Project",
                new { id = model.ProjectId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var task = await _taskRepository.GetDetailsAsync(id);

            if (task == null)
                return NotFound();

            if (!await _authorizationService.IsMemberAsync(
                    task.Project.WorkspaceId, userId))
                return Forbid();

            return View(task);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var task = await _taskRepository.GetDetailsAsync(id);

            if (task == null)
                return NotFound();

            if (!await _authorizationService.CanCreateAsync(
                    task.Project.WorkspaceId, userId))
                return Forbid();

            await LoadMembers(task.Project.WorkspaceId);

            var model = new TaskEditViewModel
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                AssignedToUserId = task.AssignedToUserId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TaskEditViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var task = await _taskRepository.GetDetailsAsync(model.Id);

            if (task == null)
                return NotFound();

            if (!await _authorizationService.CanCreateAsync(
                    task.Project.WorkspaceId, userId))
                return Forbid();

            if (!string.IsNullOrEmpty(model.AssignedToUserId))
            {
                var isAssignedUserMember =
                    await _authorizationService.IsMemberAsync(
                        task.Project.WorkspaceId,
                        model.AssignedToUserId);

                if (!isAssignedUserMember)
                {
                    ModelState.AddModelError(
                        nameof(model.AssignedToUserId),
                        "The selected user is not a member of this workspace.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadMembers(task.Project.WorkspaceId);
                return View(model);
            }

            task.Title = model.Title.Trim();
            task.Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim();
            task.Status = model.Status;
            task.Priority = model.Priority;
            task.DueDate = model.DueDate;
            task.AssignedToUserId = model.AssignedToUserId;

            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveAsync();

            TempData["Success"] = "Task updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = task.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var task = await _taskRepository.GetDetailsAsync(id);

            if (task == null)
                return NotFound();

            if (!await _authorizationService.CanCreateAsync(
                    task.Project.WorkspaceId, userId))
                return Forbid();

            var projectId = task.ProjectId;

            await _taskRepository.DeleteAsync(task);
            await _taskRepository.SaveAsync();

            TempData["Success"] = "Task deleted successfully.";

            return RedirectToAction(
                "Details",
                "Project",
                new { id = projectId });
        }

        private async Task LoadMembers(int workspaceId)
        {
            var members = await _context.WorkspaceMembers
                .Include(m => m.User)
                .Where(m => m.WorkspaceId == workspaceId)
                .OrderBy(m => m.User.FullName)
                .ToListAsync();

            ViewBag.WorkspaceMembers = members;
        }
    }
}