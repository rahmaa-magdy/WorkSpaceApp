using WorkSpaceApp.Models;

namespace WorkSpaceApp.Repositories.Interfaces
{
    public interface IWorkspaceRepository
    {
        Task<List<Workspace>> GetUserWorkspacesAsync(
            string userId);

        Task<Workspace?> GetByIdAsync(int id);

        Task<Workspace?> GetDetailsAsync(int id);

        Task AddAsync(Workspace workspace);

        Task UpdateAsync(Workspace workspace);

        Task DeleteAsync(Workspace workspace);

        Task SaveAsync();
    }
}