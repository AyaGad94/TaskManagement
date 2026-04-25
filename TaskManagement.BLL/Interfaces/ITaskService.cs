using TaskManagement.BLL.DTOs;

namespace TaskManagement.BLL.Interfaces
{
    public interface ITaskService
    {
        
        Task<IEnumerable<TaskDto>> GetAllTasks();
        Task<TaskDto?> GetTaskById(int id);
        Task<TaskDto> CreateTask(TaskCreateDto dto);
        Task<bool> UpdateTask(int id, TaskCreateDto dto);
        Task<bool> DeleteTask(int id);
    }
}
