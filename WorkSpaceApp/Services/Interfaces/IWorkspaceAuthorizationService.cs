using WorkSpaceApp.Models;

namespace WorkSpaceApp.Services.Interfaces
{
    public interface IWorkspaceAuthorizationService
    {
        Task<WorkspaceRole?> GetUserRoleAsync(
            int workspaceId,
            string userId);

        Task<bool> IsMemberAsync(
            int workspaceId,
            string userId);

        Task<bool> CanCreateAsync(
            int workspaceId,
            string userId);

        Task<bool> CanManageMembersAsync(
            int workspaceId,
            string userId);

        Task<bool> CanDeleteAsync(
            int workspaceId,
            string userId);
    }
}