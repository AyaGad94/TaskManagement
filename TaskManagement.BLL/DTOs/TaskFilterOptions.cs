using TaskManagement.DAL.Enums;

namespace TaskManagement.BLL.DTOs
{
    public class TaskFilterOptions
    {
        public UserTaskStatus? StatusFilter { get; set; }
        public DateTime? DueDateFilter { get; set; }

      
    }
}
