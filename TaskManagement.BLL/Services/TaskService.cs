using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.BLL.DTOs;
using TaskManagement.BLL.Exceptions;
using TaskManagement.BLL.Interfaces;
using TaskManagement.DAL.Entities;
using TaskManagement.DAL.Extensions;
using TaskManagement.DAL.Interfaces;

namespace TaskManagement.BLL.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskService> _logger;

        public TaskService(ITaskRepository repository, IMapper mapper , ILogger<TaskService> logger )
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }
     

        public async Task<PagedResponse<TaskDto>> GetAllTasks(TaskFilterOptions filterOptions)
        {
            _logger.LogInformation("Fetching tasks with filters: Status={Status}, DueDate={DueDate}, Page={Page}, Size={Size}",
                filterOptions.StatusFilter, filterOptions.DueDateFilter, filterOptions.PageNumber, filterOptions.PageSize);
         
            var taskQueryable = _repository.GetQueryable().AsNoTracking();

            if (filterOptions.StatusFilter.HasValue)
            {
                taskQueryable = taskQueryable.Where(task => task.Status == filterOptions.StatusFilter.Value);
            }

            if (filterOptions.DueDateFilter.HasValue)
            {
                var date = filterOptions.DueDateFilter.Value.Date;

                if (date > DateTime.UtcNow.Date)
                {
                    _logger.LogWarning("Attempted to filter by future date: {Date}", date);

                    throw new BadRequestException("Filtering by a Future Date is not allowed.");
                }

                taskQueryable = taskQueryable.Where(task => task.DueDate >= date && task.DueDate < date.AddDays(1));
            }

            var totalTasksCount = await taskQueryable.CountAsync();
            var tasksList = await taskQueryable
                .OrderByDescending(task => task.CreatedAt)
                .PageBy(filterOptions.PageNumber, filterOptions.PageSize)
                .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            _logger.LogInformation("Returned {Count} tasks out of {Total}", tasksList.Count, totalTasksCount);

            return new PagedResponse<TaskDto>(tasksList, totalTasksCount, filterOptions.PageNumber, filterOptions.PageSize);
        }


     
        public async Task<TaskDto?> GetTaskById(int id)
        {
            _logger.LogInformation("Fetching task with ID: {Id}", id);

            var task = await _repository.GetQueryable()
                .Where(t => t.Id == id)
                .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (task == null)
                _logger.LogWarning("Task with ID {Id} was not found", id);

            return task;
        }

        public async Task<TaskDto> CreateTask(TaskCreateDto dto)
        {
            _logger.LogInformation("Creating task with title: {Title}", dto.Title);

            var taskEntity = _mapper.Map<TaskItem>(dto);
            await _repository.Add(taskEntity);
            await _repository.SaveChangesAsync();
            _logger.LogInformation("Task created successfully with ID: {Id}", taskEntity.Id);

            return _mapper.Map<TaskDto>(taskEntity);
        }

        public async Task<bool> UpdateTask(int id, TaskCreateDto dto)
        {
            _logger.LogInformation("Updating task with ID: {Id}", id);

            var existingTask = await _repository.GetById(id);
            if (existingTask == null) 
            {
                _logger.LogWarning("Task with ID {Id} not found for update", id);

                return false;
            }
                
               
            _mapper.Map(dto, existingTask);
            var result = await _repository.SaveChangesAsync();
            _logger.LogInformation("Task with ID {Id} updated successfully", id);
            return result;

        }
   

        public async Task<bool> DeleteTask(int id)
        {
            _logger.LogInformation("Deleting task with ID: {Id}", id);

            var task = await _repository.GetById(id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {Id} not found for deletion", id);

                return false;
            }
                
            _repository.Delete(task);
            var result = await _repository.SaveChangesAsync();
            _logger.LogInformation("Task with ID {Id} deleted successfully", id);
            return result;

        }
        
    }
}
