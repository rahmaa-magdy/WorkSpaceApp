using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Data;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;
using WorkSpaceApp.ViewModels;

namespace WorkSpaceApp.Controllers
{
    [Authorize]
    public class WorkTaskController : Controller
    {
        private readonly IWorkTaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkTaskController(
            IWorkTaskRepository taskRepository,
            IProjectRepository projectRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int projectId)
        {
            var project =
                await _projectRepository.GetDetailsAsync(projectId);

            if (project == null)
            {
                return NotFound();
            }

            if (!IsMember(project.Workspace))
            {
                return Forbid();
            }

            await LoadMembers(project.WorkspaceId);

            ViewBag.ProjectName = project.Name;

            return View(new TaskCreateViewModel
            {
                ProjectId = projectId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TaskCreateViewModel model)
        {
            var project =
                await _projectRepository.GetDetailsAsync(
                    model.ProjectId);

            if (project == null)
            {
                return NotFound();
            }

            if (!IsMember(project.Workspace))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await LoadMembers(project.WorkspaceId);
                ViewBag.ProjectName = project.Name;
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.AssignedToUserId))
            {
                var isWorkspaceMember =
                    project.Workspace.Members
                        .Any(m =>
                            m.UserId == model.AssignedToUserId);

                if (!isWorkspaceMember)
                {
                    ModelState.AddModelError(
                        nameof(model.AssignedToUserId),
                        "The selected user is not a workspace member.");

                    await LoadMembers(project.WorkspaceId);
                    ViewBag.ProjectName = project.Name;

                    return View(model);
                }
            }

            var task = new WorkTask
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                DueDate = model.DueDate,
                AssignedToUserId = model.AssignedToUserId,
                ProjectId = model.ProjectId,
                Status = TaskStatus.ToDo
            };

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveAsync();

            return RedirectToAction(
                "Details",
                "Project",
                new { id = model.ProjectId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var task =
                await _taskRepository.GetDetailsAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            if (!IsMember(task.Project.Workspace))
            {
                return Forbid();
            }

            return View(task);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task =
                await _taskRepository.GetDetailsAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            if (!IsMember(task.Project.Workspace))
            {
                return Forbid();
            }

            await LoadMembers(task.Project.WorkspaceId);

            ViewBag.ProjectName = task.Project.Name;

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
        public async Task<IActionResult> Edit(
            TaskEditViewModel model)
        {
            var task =
                await _taskRepository.GetDetailsAsync(model.Id);

            if (task == null)
            {
                return NotFound();
            }

            if (!IsMember(task.Project.Workspace))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await LoadMembers(task.Project.WorkspaceId);
                ViewBag.ProjectName = task.Project.Name;
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.AssignedToUserId))
            {
                var isWorkspaceMember =
                    task.Project.Workspace.Members
                        .Any(m =>
                            m.UserId == model.AssignedToUserId);

                if (!isWorkspaceMember)
                {
                    ModelState.AddModelError(
                        nameof(model.AssignedToUserId),
                        "The selected user is not a workspace member.");

                    await LoadMembers(task.Project.WorkspaceId);
                    ViewBag.ProjectName = task.Project.Name;

                    return View(model);
                }
            }

            task.Title = model.Title;
            task.Description = model.Description;
            task.Status = model.Status;
            task.Priority = model.Priority;
            task.DueDate = model.DueDate;
            task.AssignedToUserId = model.AssignedToUserId;

            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = task.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var task =
                await _taskRepository.GetDetailsAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            if (!IsMember(task.Project.Workspace))
            {
                return Forbid();
            }

            var projectId = task.ProjectId;

            await _taskRepository.DeleteAsync(task);
            await _taskRepository.SaveAsync();

            return RedirectToAction(
                "Details",
                "Project",
                new { id = projectId });
        }

        private bool IsMember(Workspace workspace)
        {
            var userId = _userManager.GetUserId(User);

            return workspace.Members
                .Any(m => m.UserId == userId);
        }

        private async Task LoadMembers(int workspaceId)
        {
            var members = await _context.WorkspaceMembers
                .Where(m => m.WorkspaceId == workspaceId)
                .Include(m => m.User)
                .OrderBy(m => m.User!.FullName)
                .ToListAsync();

            ViewBag.Members = members;
        }
    }
}