using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Data;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;

namespace WorkSpaceApp.Repositories
{
    public class WorkTaskRepository : IWorkTaskRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkTaskRepository(ApplicationDbContext context)
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