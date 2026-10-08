using System.ComponentModel.DataAnnotations;
using WorkSpaceApp.Models;

namespace WorkSpaceApp.ViewModels
{
    public class ProjectEditViewModel
    {
        public int Id { get; set; }

        public int WorkspaceId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }

        public ProjectStatus Status { get; set; }
    }
}