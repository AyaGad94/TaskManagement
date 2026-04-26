using TaskManagement.BLL.DTOs;
using TaskManagement.BLL.Interfaces;
using TaskManagement.DAL.Entities;
using TaskManagement.DAL.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TaskManagement.DAL.Extensions;

namespace TaskManagement.BLL.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly IMapper _mapper;
        public TaskService(ITaskRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
     

        public async Task<PagedResponse<TaskDto>> GetAllTasks(TaskFilterOptions filterOptions)
        {
            var taskQueryable = _repository.GetQueryable().AsNoTracking();

            if (filterOptions.StatusFilter.HasValue)
            {
                taskQueryable = taskQueryable.Where(task => task.Status == filterOptions.StatusFilter.Value);
            }

            if (filterOptions.DueDateFilter.HasValue)
            {
                var date = filterOptions.DueDateFilter.Value.Date;

                if (date > DateTime.UtcNow.Date) throw new ArgumentException("Filtering by a future date is not allowed.");

                taskQueryable = taskQueryable.Where(task => task.DueDate >= date && task.DueDate < date.AddDays(1));
            }

            var totalTasksCount = await taskQueryable.CountAsync();

            const int fixedPageNumber = 1;
            const int strictLimitSize = 10;

            var tasksList = await taskQueryable
                .OrderByDescending(task => task.CreatedAt)
                .PageBy(fixedPageNumber, strictLimitSize)
                .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PagedResponse<TaskDto>(tasksList, totalTasksCount, fixedPageNumber, strictLimitSize);
        }


        public async Task<TaskDto?> GetTaskById(int id)
        {
            return await _repository.GetQueryable()
                  .Where(t => t.Id == id)
                  .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
                 .FirstOrDefaultAsync();
        }
        
        public async Task<TaskDto> CreateTask(TaskCreateDto dto)
        {
            var taskentity = _mapper.Map<TaskItem>(dto);
            await _repository.Add(taskentity);
            await _repository.SaveChangesAsync();
            return _mapper.Map<TaskDto>(taskentity);
        }

        public async Task<bool> UpdateTask(int id, TaskCreateDto dto)
        {
            var existingTask = await _repository.GetById(id);
            if (existingTask == null) return false;
            _mapper.Map(dto, existingTask);
            return await _repository.SaveChangesAsync();
        }
   

        public async Task<bool> DeleteTask(int id)
        {
            var task = await _repository.GetById(id);
            if (task == null) return false;
            _repository.Delete(task);
            return await _repository.SaveChangesAsync();
        }
        
    }
}
