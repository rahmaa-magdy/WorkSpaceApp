using System.ComponentModel.DataAnnotations;
using WorkSpaceApp.Models;

namespace WorkSpaceApp.ViewModels
{
    public class AddMemberViewModel
    {
        public int WorkspaceId { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        public WorkspaceRole Role { get; set; }
            = WorkspaceRole.Member;
    }
}