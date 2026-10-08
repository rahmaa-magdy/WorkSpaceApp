using WorkSpaceApp.Models;

namespace WorkSpaceApp.ViewModels
{
    public class ProjectTasksViewModel
    {
        public Project Project { get; set; } = null!;

        public List<WorkTask> Tasks { get; set; } = new();

        public string? Search { get; set; }

        public WorkSpaceApp.Models.TaskStatus? Status { get; set; }

        public TaskPriority? Priority { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalItems { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling(
                TotalItems / (double)PageSize);
    }
}