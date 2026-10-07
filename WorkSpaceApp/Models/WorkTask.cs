using System.ComponentModel.DataAnnotations;

namespace WorkSpaceApp.Models
{
    public class WorkTask
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        public int ProjectId { get; set; }

        public string? AssignedToUserId { get; set; }

        public Project Project { get; set; } = null!;

        public ApplicationUser? AssignedToUser { get; set; }

        public ICollection<Comment> Comments { get; set; }
            = new List<Comment>();
    }

    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Done
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }
}