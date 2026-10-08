using System.ComponentModel.DataAnnotations;

namespace WorkSpaceApp.ViewModels
{
    public class CommentCreateViewModel
    {
        public int WorkTaskId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}