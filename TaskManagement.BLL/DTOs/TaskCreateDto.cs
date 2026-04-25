using System.ComponentModel.DataAnnotations;
using TaskManagement.DAL.Enums;


namespace TaskManagement.BLL.DTOs
{
    public class TaskCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [EnumDataType(typeof(UserTaskStatus))]
        public UserTaskStatus Status { get; set; } = UserTaskStatus.Pending;
        [Required]
        public DateTime DueDate { get; set; }
    }
}
