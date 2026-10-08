using System.ComponentModel.DataAnnotations;

namespace WorkSpaceApp.ViewModels
{
    public class WorkspaceEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Workspace name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}