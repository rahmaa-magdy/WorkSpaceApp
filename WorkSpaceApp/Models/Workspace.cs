using System.ComponentModel.DataAnnotations;

namespace WorkSpaceApp.Models
{
    public class Workspace
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WorkspaceMember> Members { get; set; }
            = new List<WorkspaceMember>();

        public ICollection<Project> Projects { get; set; }
            = new List<Project>();
    }
}