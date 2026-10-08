using System.ComponentModel.DataAnnotations;
using WorkSpaceApp.Models;

namespace WorkSpaceApp.ViewModels
{
    public class ProjectCreateViewModel
    {
        public int WorkspaceId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Project name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Deadline")]
        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }

        public ProjectStatus Status { get; set; }
            = ProjectStatus.NotStarted;
    }
}