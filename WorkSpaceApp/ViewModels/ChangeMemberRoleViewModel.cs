using System.ComponentModel.DataAnnotations;
using WorkSpaceApp.Models;

namespace WorkSpaceApp.ViewModels
{
    public class ChangeMemberRoleViewModel
    {
        public int WorkspaceId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public WorkspaceRole Role { get; set; }
    }
}