using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;
using WorkSpaceApp.ViewModels;

namespace WorkSpaceApp.Controllers
{
    [Authorize]
    public class WorkspaceMemberController : Controller
    {
        private readonly IWorkspaceMemberRepository _memberRepository;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkspaceMemberController(
            IWorkspaceMemberRepository memberRepository,
            IWorkspaceRepository workspaceRepository,
            UserManager<ApplicationUser> userManager)
        {
            _memberRepository = memberRepository;
            _workspaceRepository = workspaceRepository;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            AddMemberViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            var currentMembership =
                await _memberRepository.GetMembershipAsync(
                    model.WorkspaceId,
                    userId!);

            if (currentMembership == null)
            {
                return Forbid();
            }

            if (currentMembership.Role != WorkspaceRole.Owner &&
                currentMembership.Role != WorkspaceRole.Manager)
            {
                return Forbid();
            }

            var user =
                await _userManager.FindByEmailAsync(
                    model.Email);

            if (user == null)
            {
                TempData["Error"] =
                    "No user was found with this email.";

                return RedirectToAction(
                    "Details",
                    "Workspace",
                    new { id = model.WorkspaceId });
            }

            var existing =
                await _memberRepository.GetMembershipAsync(
                    model.WorkspaceId,
                    user.Id);

            if (existing != null)
            {
                TempData["Error"] =
                    "This user is already a workspace member.";

                return RedirectToAction(
                    "Details",
                    "Workspace",
                    new { id = model.WorkspaceId });
            }

            var member = new WorkspaceMember
            {
                WorkspaceId = model.WorkspaceId,
                UserId = user.Id,
                Role = model.Role
            };

            await _memberRepository.AddAsync(member);
            await _memberRepository.SaveAsync();

            TempData["Success"] =
                $"{user.FullName} was added to the workspace.";

            return RedirectToAction(
                "Details",
                "Workspace",
                new { id = model.WorkspaceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            int workspaceId,
            string userId)
        {
            var currentUserId =
                _userManager.GetUserId(User);

            var currentMembership =
                await _memberRepository.GetMembershipAsync(
                    workspaceId,
                    currentUserId!);

            if (currentMembership?.Role != WorkspaceRole.Owner)
            {
                return Forbid();
            }

            var targetMembership =
                await _memberRepository.GetMembershipAsync(
                    workspaceId,
                    userId);

            if (targetMembership == null)
            {
                return NotFound();
            }

            if (targetMembership.Role == WorkspaceRole.Owner)
            {
                TempData["Error"] =
                    "The workspace owner cannot be removed.";

                return RedirectToAction(
                    "Details",
                    "Workspace",
                    new { id = workspaceId });
            }

            await _memberRepository
                .RemoveAsync(targetMembership);

            await _memberRepository.SaveAsync();

            TempData["Success"] =
                "Member removed from the workspace.";

            return RedirectToAction(
                "Details",
                "Workspace",
                new { id = workspaceId });
        }
    }
}