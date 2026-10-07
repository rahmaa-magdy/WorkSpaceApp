using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Data;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;

namespace WorkSpaceApp.Repositories
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkspaceRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Workspace>>
            GetUserWorkspacesAsync(string userId)
        {
            return await _context.Workspaces
                .Include(w => w.Members)
                .Where(w => w.Members
                    .Any(m => m.UserId == userId))
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<Workspace?> GetByIdAsync(int id)
        {
            return await _context.Workspaces
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Workspace?> GetDetailsAsync(int id)
        {
            return await _context.Workspaces
                .Include(w => w.Members)
                    .ThenInclude(m => m.User)
                .Include(w => w.Projects)
                    .ThenInclude(p => p.Tasks)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task AddAsync(Workspace workspace)
        {
            await _context.Workspaces.AddAsync(workspace);
        }

        public async Task UpdateAsync(Workspace workspace)
        {
            _context.Workspaces.Update(workspace);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Workspace workspace)
        {
            _context.Workspaces.Remove(workspace);
            await Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}