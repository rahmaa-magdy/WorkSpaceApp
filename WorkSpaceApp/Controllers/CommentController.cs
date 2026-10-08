using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;
using WorkSpaceApp.ViewModels;

namespace WorkSpaceApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IWorkTaskRepository _taskRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(
            ICommentRepository commentRepository,
            IWorkTaskRepository taskRepository,
            UserManager<ApplicationUser> userManager)
        {
            _commentRepository = commentRepository;
            _taskRepository = taskRepository;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CommentCreateViewModel model)
        {
            var task =
                await _taskRepository.GetDetailsAsync(
                    model.WorkTaskId);

            if (task == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var isMember = task.Project.Workspace.Members
                .Any(m => m.UserId == userId);

            if (!isMember)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction(
                    "Details",
                    "WorkTask",
                    new { id = model.WorkTaskId });
            }

            var comment = new Comment
            {
                Content = model.Content,
                WorkTaskId = model.WorkTaskId,
                UserId = userId!
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveAsync();

            return RedirectToAction(
                "Details",
                "WorkTask",
                new { id = model.WorkTaskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comment =
                await _commentRepository.GetByIdAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var isMember = comment.WorkTask.Project.Workspace
                .Members
                .Any(m => m.UserId == userId);

            if (!isMember)
            {
                return Forbid();
            }

            // A user can only delete their own comment.
            if (comment.UserId != userId)
            {
                return Forbid();
            }

            var taskId = comment.WorkTaskId;

            await _commentRepository.DeleteAsync(comment);
            await _commentRepository.SaveAsync();

            return RedirectToAction(
                "Details",
                "WorkTask",
                new { id = taskId });
        }
    }
}
