using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkSpaceApp.Models;

namespace WorkSpaceApp.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider services)
        {
            var context =
                services.GetRequiredService<ApplicationDbContext>();

            var userManager =
                services.GetRequiredService<
                    UserManager<ApplicationUser>>();

            await context.Database.MigrateAsync();


           
            var owner =
                await CreateUserAsync(
                    userManager,
                    "owner@workspace.local",
                    "Workspace Owner",
                    "Owner123!");

            var manager =
                await CreateUserAsync(
                    userManager,
                    "manager@workspace.local",
                    "Project Manager",
                    "Manager123!");

            var member =
                await CreateUserAsync(
                    userManager,
                    "member@workspace.local",
                    "Team Member",
                    "Member123!");


            //workspace
            var workspace =
                await context.Workspaces
                    .FirstOrDefaultAsync(
                        w => w.Name == "Product Development");

            if (workspace == null)
            {
                workspace = new Workspace
                {
                    Name = "Product Development",
                    Description =
                        "A demo workspace for managing product development, projects and team tasks."
                };

                context.Workspaces.Add(workspace);

                await context.SaveChangesAsync();
            }

            
            //member
            await EnsureMemberAsync(
                context,
                workspace.Id,
                owner.Id,
                WorkspaceRole.Owner);

            await EnsureMemberAsync(
                context,
                workspace.Id,
                manager.Id,
                WorkspaceRole.Manager);

            await EnsureMemberAsync(
                context,
                workspace.Id,
                member.Id,
                WorkspaceRole.Member);


            //project
            var websiteProject =
                await context.Projects
                    .FirstOrDefaultAsync(
                        p =>
                            p.WorkspaceId == workspace.Id &&
                            p.Name == "Company Website");

            if (websiteProject == null)
            {
                websiteProject = new Project
                {
                    Name = "Company Website",
                    Description =
                        "Build and launch the new company website.",
                    WorkspaceId = workspace.Id,
                    Status = ProjectStatus.InProgress,
                    Deadline = DateTime.UtcNow.AddDays(30)
                };

                context.Projects.Add(websiteProject);

                await context.SaveChangesAsync();
            }


            var mobileProject =
                await context.Projects
                    .FirstOrDefaultAsync(
                        p =>
                            p.WorkspaceId == workspace.Id &&
                            p.Name == "Mobile App");

            if (mobileProject == null)
            {
                mobileProject = new Project
                {
                    Name = "Mobile App",
                    Description =
                        "Prepare the first version of the mobile application.",
                    WorkspaceId = workspace.Id,
                    Status = ProjectStatus.NotStarted,
                    Deadline = DateTime.UtcNow.AddDays(60)
                };

                context.Projects.Add(mobileProject);

                await context.SaveChangesAsync();
            }


           
            //tasks
            await EnsureTaskAsync(
                context,
                websiteProject.Id,
                "Create landing page",
                "Implement the main landing page and responsive layout.",
                WorkSpaceApp.Models.TaskStatus.InProgress,
                TaskPriority.High,
                DateTime.UtcNow.AddDays(5),
                manager.Id);

            await EnsureTaskAsync(
                context,
                websiteProject.Id,
                "Prepare database",
                "Create the initial database structure.",
                WorkSpaceApp.Models.TaskStatus.Done,
                TaskPriority.High,
                DateTime.UtcNow.AddDays(-2),
                owner.Id);

            await EnsureTaskAsync(
                context,
                websiteProject.Id,
                "Write API documentation",
                "Document the main backend endpoints.",
                WorkSpaceApp.Models.TaskStatus.ToDo,
                TaskPriority.Medium,
                DateTime.UtcNow.AddDays(12),
                member.Id);

            await EnsureTaskAsync(
                context,
                mobileProject.Id,
                "Create mobile wireframes",
                "Prepare the initial application wireframes.",
                WorkSpaceApp.Models.TaskStatus.ToDo,
                TaskPriority.Low,
                DateTime.UtcNow.AddDays(20),
                member.Id);

            await EnsureTaskAsync(
                context,
                mobileProject.Id,
                "Define authentication flow",
                "Design the login and registration flow.",
                WorkSpaceApp.Models.TaskStatus.InProgress,
                TaskPriority.High,
                DateTime.UtcNow.AddDays(15),
                manager.Id);


            var existingComment =
                await context.Comments.FirstOrDefaultAsync();

            if (existingComment == null)
            {
                var task =
                    await context.WorkTasks
                        .FirstAsync(
                            t => t.Title == "Create landing page");

                context.Comments.Add(
                    new Comment
                    {
                        Content =
                            "The first version of the landing page is ready for review.",
                        WorkTaskId = task.Id,
                        UserId = manager.Id
                    });

                await context.SaveChangesAsync();
            }
        }


        private static async Task<ApplicationUser>
            CreateUserAsync(
                UserManager<ApplicationUser> userManager,
                string email,
                string fullName,
                string password)
        {
            var existingUser =
                await userManager.FindByEmailAsync(email);

            if (existingUser != null)
                return existingUser;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                CreatedAt = DateTime.UtcNow
            };

            var result =
                await userManager.CreateAsync(
                    user,
                    password);

            if (!result.Succeeded)
            {
                var errors =
                    string.Join(
                        "; ",
                        result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Could not create seed user {email}: {errors}");
            }

            return user;
        }

        
        private static async Task EnsureMemberAsync(
            ApplicationDbContext context,
            int workspaceId,
            string userId,
            WorkspaceRole role)
        {
            var existing =
                await context.WorkspaceMembers
                    .FirstOrDefaultAsync(
                        m =>
                            m.WorkspaceId == workspaceId &&
                            m.UserId == userId);

            if (existing == null)
            {
                context.WorkspaceMembers.Add(
                    new WorkspaceMember
                    {
                        WorkspaceId = workspaceId,
                        UserId = userId,
                        Role = role
                    });

                await context.SaveChangesAsync();
            }
            else if (existing.Role != role)
            {
                existing.Role = role;

                await context.SaveChangesAsync();
            }
        }


        private static async Task EnsureTaskAsync(
            ApplicationDbContext context,
            int projectId,
            string title,
            string description,
            WorkSpaceApp.Models.TaskStatus status,
            TaskPriority priority,
            DateTime? dueDate,
            string? assignedUserId)
        {
            var exists =
                await context.WorkTasks
                    .AnyAsync(
                        t =>
                            t.ProjectId == projectId &&
                            t.Title == title);

            if (exists)
                return;

            context.WorkTasks.Add(
                new WorkTask
                {
                    ProjectId = projectId,
                    Title = title,
                    Description = description,
                    Status = status,
                    Priority = priority,
                    DueDate = dueDate,
                    AssignedToUserId = assignedUserId
                });

            await context.SaveChangesAsync();
        }
    }
}