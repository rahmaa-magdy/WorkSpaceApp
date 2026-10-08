using System.ComponentModel.DataAnnotations;
using WorkSpaceApp.Models;

namespace WorkSpaceApp.ViewModels
{
    public class TaskCreateViewModel
    {
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; }
            = TaskPriority.Medium;

        public DateTime? DueDate { get; set; }

        public string? AssignedToUserId { get; set; }
    }
}