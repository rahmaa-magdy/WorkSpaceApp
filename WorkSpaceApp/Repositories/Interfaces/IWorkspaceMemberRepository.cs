using WorkSpaceApp.Models;

namespace WorkSpaceApp.Repositories.Interfaces
{
    public interface IWorkspaceMemberRepository
    {
        Task AddAsync(WorkspaceMember member);

        Task<List<WorkspaceMember>>
            GetWorkspaceMembersAsync(int workspaceId);

        Task<WorkspaceMember?>
            GetMembershipAsync(
                int workspaceId,
                string userId);

        Task<bool> IsMemberAsync(
            int workspaceId,
            string userId);

        Task RemoveAsync(
            WorkspaceMember member);

        Task SaveAsync();
    }
}