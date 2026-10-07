using System.ComponentModel.DataAnnotations;

namespace WorkSpaceApp.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? Deadline { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.NotStarted;

        public int WorkspaceId { get; set; }

        public Workspace Workspace { get; set; } = null!;

        public ICollection<WorkTask> Tasks { get; set; }
            = new List<WorkTask>();
    }

    public enum ProjectStatus
    {
        NotStarted,
        InProgress,
        Completed,
        OnHold
    }
}
