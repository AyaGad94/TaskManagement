using TaskManagement.DAL.Entities;

namespace TaskManagement.DAL.Interfaces
{
    public interface ITaskRepository
    {
        IQueryable<TaskItem> GetQueryable();
        Task<TaskItem?> GetById(int id);
        Task Add(TaskItem task);
        void Update(TaskItem task);
        void Delete(TaskItem task);
        Task<bool> SaveChangesAsync();
    }
}
