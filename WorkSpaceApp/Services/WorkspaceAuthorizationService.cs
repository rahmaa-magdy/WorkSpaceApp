using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Data;
using WorkSpaceApp.Models;
using WorkSpaceApp.Services.Interfaces;

namespace WorkSpaceApp.Services
{
    public class WorkspaceAuthorizationService
        : IWorkspaceAuthorizationService
    {
        private readonly ApplicationDbContext _context;

        public WorkspaceAuthorizationService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WorkspaceRole?> GetUserRoleAsync(
            int workspaceId,
            string userId)
        {
            return await _context.WorkspaceMembers
                .Where(m =>
                    m.WorkspaceId == workspaceId &&
                    m.UserId == userId)
                .Select(m => (WorkspaceRole?)m.Role)
                .FirstOrDefaultAsync();
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

        public async Task<bool> CanCreateAsync(
            int workspaceId,
            string userId)
        {
            var role = await GetUserRoleAsync(
                workspaceId,
                userId);

            return role == WorkspaceRole.Owner ||
                   role == WorkspaceRole.Manager;
        }

        public async Task<bool> CanManageMembersAsync(
            int workspaceId,
            string userId)
        {
            var role = await GetUserRoleAsync(
                workspaceId,
                userId);

            return role == WorkspaceRole.Owner ||
                   role == WorkspaceRole.Manager;
        }

        public async Task<bool> CanDeleteAsync(
            int workspaceId,
            string userId)
        {
            var role = await GetUserRoleAsync(
                workspaceId,
                userId);

            return role == WorkspaceRole.Owner;
        }
    }
}