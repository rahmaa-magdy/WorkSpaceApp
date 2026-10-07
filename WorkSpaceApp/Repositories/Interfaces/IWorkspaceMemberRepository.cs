using WorkSpaceApp.Models;

namespace WorkSpaceApp.Repositories.Interfaces
{
    public interface IWorkspaceMemberRepository
    {
        Task AddAsync(WorkspaceMember member);

        Task<List<WorkspaceMember>>
            GetWorkspaceMembersAsync(int workspaceId);

        Task<bool> IsMemberAsync(
            int workspaceId,
            string userId);

        Task SaveAsync();
    }
}