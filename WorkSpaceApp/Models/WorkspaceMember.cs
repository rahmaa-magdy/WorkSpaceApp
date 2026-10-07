namespace WorkSpaceApp.Models
{
    public class WorkspaceMember
    {
        public int Id { get; set; }

        public int WorkspaceId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public WorkspaceRole Role { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public Workspace Workspace { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;
    }

    public enum WorkspaceRole
    {
        Member,
        Manager,
        Owner
    }
}