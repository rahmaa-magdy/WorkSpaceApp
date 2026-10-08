using WorkSpaceApp.Models;

namespace WorkSpaceApp.Repositories.Interfaces
{
    public interface IWorkTaskRepository
    {
        Task<List<WorkTask>> GetProjectTasksAsync(int projectId);

        Task<(List<WorkTask> Tasks, int TotalItems)>
            SearchProjectTasksAsync(
                int projectId,
                string? search,
                WorkSpaceApp.Models.TaskStatus? status,
                TaskPriority? priority,
                int page,
                int pageSize);

        Task<WorkTask?> GetDetailsAsync(int id);

        Task<WorkTask?> GetByIdAsync(int id);

        Task AddAsync(WorkTask task);

        Task UpdateAsync(WorkTask task);

        Task DeleteAsync(WorkTask task);

        Task SaveAsync();
    }
}