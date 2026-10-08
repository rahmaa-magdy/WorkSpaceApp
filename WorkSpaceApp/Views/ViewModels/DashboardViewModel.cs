using WorkSpaceApp.Models;

namespace WorkSpaceApp.ViewModels
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;

        public int WorkspaceCount { get; set; }

        public int ActiveProjectCount { get; set; }

        public int OpenTaskCount { get; set; }

        public int CompletedTaskCount { get; set; }

        public List<WorkTask> RecentTasks { get; set; }
            = new();

        public List<WorkTask> UpcomingTasks { get; set; }
            = new();
    }
}