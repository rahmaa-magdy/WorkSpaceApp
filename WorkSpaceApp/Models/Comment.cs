using System.ComponentModel.DataAnnotations;

namespace WorkSpaceApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int WorkTaskId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public WorkTask WorkTask { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;
    }
}