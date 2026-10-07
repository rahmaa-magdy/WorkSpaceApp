using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Models;

namespace WorkSpaceApp.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Workspace> Workspaces { get; set; }

        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<WorkTask> WorkTasks { get; set; }

        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // WorkspaceMember -> Workspace
            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.Workspace)
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // WorkspaceMember -> User
            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.User)
                .WithMany(u => u.WorkspaceMemberships)
                .HasForeignKey(wm => wm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent duplicate membership
            modelBuilder.Entity<WorkspaceMember>()
                .HasIndex(wm => new
                {
                    wm.WorkspaceId,
                    wm.UserId
                })
                .IsUnique();

            // Workspace -> Projects
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Workspace)
                .WithMany(w => w.Projects)
                .HasForeignKey(p => p.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project -> Tasks
            modelBuilder.Entity<WorkTask>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Task -> Assigned User
            modelBuilder.Entity<WorkTask>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Task -> Comments
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.WorkTask)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment -> User
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}