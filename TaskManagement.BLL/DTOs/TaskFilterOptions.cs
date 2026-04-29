using TaskManagement.DAL.Enums;

namespace TaskManagement.BLL.DTOs
{
    public class TaskFilterOptions
    {
        public UserTaskStatus? StatusFilter { get; set; }
        public DateTime? DueDateFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
