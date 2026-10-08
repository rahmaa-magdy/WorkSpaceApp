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
    public class WorkspaceMemberController : Controller
    {
        private readonly IWorkspaceMemberRepository _memberRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWorkspaceAuthorizationService _authorizationService;

        public WorkspaceMemberController(
            IWorkspaceMemberRepository memberRepository,
            UserManager<ApplicationUser> userManager,
            IWorkspaceAuthorizationService authorizationService)
        {
            _memberRepository = memberRepository;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }


        //add members
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddMemberViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
                return Challenge();

            //owner or manager only
            if (!await _authorizationService.CanManageMembersAsync(
                    model.WorkspaceId,
                    currentUserId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Please enter a valid email address.";

                return RedirectToAction(
                    "Details",
                    "Workspace",
                    new { id = model.WorkspaceId });
            }

            //Find users by email
            var user = await _userManager.FindByEmailAsync(
                model.Email.Trim());

            if (user == null)
            {
                TempData["Error"] =
                    "No registered user was found with this email.";

                return RedirectToAction(
                    "Details",
                    "Workspace",
                    new { id = model.WorkspaceId });
            }

            //check existing
            var existingMember =
                await _memberRepository.GetMembershipAsync(
                    model.WorkspaceId,
                    user.Id);

            if (existingMember != null)
            {
                TempData["Error"] =
                    "This user is already a workspace member.";

                return RedirectToAction(
                    "Details",
                    "Workspace",
                    new { id = model.WorkspaceId });
            }

            //create membership
            var member = new WorkspaceMember
            {
                WorkspaceId = model.WorkspaceId,
                UserId = user.Id,
                Role = model.Role,
                JoinedAt = DateTime.UtcNow
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


        //change role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(
            ChangeMemberRoleViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
                return Challenge();

            //owner only
            if (!await _authorizationService.CanDeleteAsync(
                    model.WorkspaceId,
                    currentUserId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Invalid member role information.";

                return RedirectToAction(
                    "Details",
                    "Workspace",
                    new { id = model.WorkspaceId });
            }

            
            var targetMembership =
                await _memberRepository.GetMembershipAsync(
                    model.WorkspaceId,
                    model.UserId);

            if (targetMembership == null)
                return NotFound();

            //can't allow changing owner's role
            if (targetMembership.Role == WorkspaceRole.Owner)
            {
                TempData["Error"] =
                    "The workspace owner's role cannot be changed.";

                return RedirectToAction(
                    "Details",
                    "Workspace",
                    new { id = model.WorkspaceId });
            }

            //prevent assigning owner role
            if (model.Role == WorkspaceRole.Owner)
            {
                TempData["Error"] =
                    "The Owner role cannot be assigned through this action.";

                return RedirectToAction(
                    "Details",
                    "Workspace",
                    new { id = model.WorkspaceId });
            }

            targetMembership.Role = model.Role;

            await _memberRepository.SaveAsync();

            TempData["Success"] =
                "Member role updated successfully.";

            return RedirectToAction(
                "Details",
                "Workspace",
                new { id = model.WorkspaceId });
        }


        //remove member
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            int workspaceId,
            string userId)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
                return Challenge();

            //owner only
            if (!await _authorizationService.CanDeleteAsync(
                    workspaceId,
                    currentUserId))
            {
                return Forbid();
            }

            var targetMembership =
                await _memberRepository.GetMembershipAsync(
                    workspaceId,
                    userId);

            if (targetMembership == null)
                return NotFound();

            //owner cannot be removed
            if (targetMembership.Role == WorkspaceRole.Owner)
            {
                TempData["Error"] =
                    "The workspace owner cannot be removed.";

                return RedirectToAction(
                    "Details",
                    "Workspace",
                    new { id = workspaceId });
            }

            await _memberRepository.RemoveAsync(
                targetMembership);

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