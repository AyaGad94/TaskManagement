using TaskManagement.DAL.Enums;

namespace TaskManagement.BLL.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public UserTaskStatus Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
