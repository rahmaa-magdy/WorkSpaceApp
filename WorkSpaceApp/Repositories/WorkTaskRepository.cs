using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Data;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;

namespace WorkSpaceApp.Repositories
{
    public class WorkTaskRepository : IWorkTaskRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkTaskRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkTask>> GetProjectTasksAsync(
            int projectId)
        {
            return await _context.WorkTasks
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.AssignedToUser)
                .OrderBy(t => t.Status)
                .ThenBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<(List<WorkTask> Tasks, int TotalItems)>
            SearchProjectTasksAsync(
                int projectId,
                string? search,
                WorkSpaceApp.Models.TaskStatus? status,
                TaskPriority? priority,
                int page,
                int pageSize)
        {
            var query = _context.WorkTasks
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.AssignedToUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    (t.Description != null &&
                     t.Description.Contains(search)));
            }

            if (status.HasValue)
            {
                query = query.Where(t =>
                    t.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(t =>
                    t.Priority == priority.Value);
            }

            var totalItems = await query.CountAsync();

            var tasks = await query
                .OrderBy(t => t.Status)
                .ThenBy(t => t.DueDate)
                .ThenByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tasks, totalItems);
        }

        public async Task<WorkTask?> GetByIdAsync(int id)
        {
            return await _context.WorkTasks
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<WorkTask?> GetDetailsAsync(int id)
        {
            return await _context.WorkTasks

                .Include(t => t.Project)
                    .ThenInclude(p => p.Workspace)
                        .ThenInclude(w => w.Members)
                            .ThenInclude(m => m.User)

                .Include(t => t.AssignedToUser)

                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)

                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAsync(WorkTask task)
        {
            await _context.WorkTasks.AddAsync(task);
        }

        public async Task UpdateAsync(WorkTask task)
        {
            _context.WorkTasks.Update(task);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(WorkTask task)
        {
            _context.WorkTasks.Remove(task);
            await Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}