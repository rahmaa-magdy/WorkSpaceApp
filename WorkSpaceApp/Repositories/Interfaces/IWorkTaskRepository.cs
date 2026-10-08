using WorkSpaceApp.Models;

namespace WorkSpaceApp.Repositories.Interfaces
{
    public interface IWorkTaskRepository
    {
        Task<List<WorkTask>> GetProjectTasksAsync(int projectId);

        Task<WorkTask?> GetDetailsAsync(int id);

        Task<WorkTask?> GetByIdAsync(int id);

        Task AddAsync(WorkTask task);

        Task UpdateAsync(WorkTask task);

        Task DeleteAsync(WorkTask task);

        Task SaveAsync();
    }
}