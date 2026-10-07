using Microsoft.AspNetCore.Identity;

namespace WorkSpaceApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; }
            = new List<WorkspaceMember>();

        public ICollection<WorkTask> AssignedTasks { get; set; }
            = new List<WorkTask>();

        public ICollection<Comment> Comments { get; set; }
            = new List<Comment>();
    }
}