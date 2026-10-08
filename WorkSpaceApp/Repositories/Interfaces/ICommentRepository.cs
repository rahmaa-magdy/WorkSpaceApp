using WorkSpaceApp.Models;

namespace WorkSpaceApp.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(int id);

        Task AddAsync(Comment comment);

        Task DeleteAsync(Comment comment);

        Task SaveAsync();
    }
}
