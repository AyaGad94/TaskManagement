using TaskManagement.BLL.DTOs;
using TaskManagement.BLL.Interfaces;
using TaskManagement.DAL.Entities;
using TaskManagement.DAL.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IEnumerable<TaskDto>> GetAllTasks()
        {
           
            return await _repository.GetQueryable()
                .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
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
