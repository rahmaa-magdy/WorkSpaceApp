using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Data;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;

namespace WorkSpaceApp.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>>
            GetWorkspaceProjectsAsync(int workspaceId)
        {
            return await _context.Projects
                .Where(p => p.WorkspaceId == workspaceId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project?> GetDetailsAsync(int id)
        {
            return await _context.Projects

                .Include(p => p.Workspace)

                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedToUser)

                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Comments)

                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
        }

        public async Task UpdateAsync(Project project)
        {
            _context.Projects.Update(project);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Project project)
        {
            _context.Projects.Remove(project);
            await Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}