using WorkSpaceApp.Models;

namespace WorkSpaceApp.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetWorkspaceProjectsAsync(
            int workspaceId);

        Task<Project?> GetDetailsAsync(int id);

        Task<Project?> GetByIdAsync(int id);

        Task AddAsync(Project project);

        Task UpdateAsync(Project project);

        Task DeleteAsync(Project project);

        Task SaveAsync();
    }
}