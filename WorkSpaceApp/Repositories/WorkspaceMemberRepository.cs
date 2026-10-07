using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Data;
using WorkSpaceApp.Models;
using WorkSpaceApp.Repositories.Interfaces;

namespace WorkSpaceApp.Repositories
{
    public class WorkspaceMemberRepository
        : IWorkspaceMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkspaceMemberRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            WorkspaceMember member)
        {
            await _context.WorkspaceMembers
                .AddAsync(member);
        }

        public async Task<List<WorkspaceMember>>
            GetWorkspaceMembersAsync(int workspaceId)
        {
            return await _context.WorkspaceMembers
                .Include(m => m.User)
                .Where(m => m.WorkspaceId == workspaceId)
                .ToListAsync();
        }

        public async Task<bool> IsMemberAsync(
            int workspaceId,
            string userId)
        {
            return await _context.WorkspaceMembers
                .AnyAsync(m =>
                    m.WorkspaceId == workspaceId &&
                    m.UserId == userId);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}