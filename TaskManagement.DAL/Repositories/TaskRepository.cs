using Microsoft.EntityFrameworkCore;
using TaskManagement.DAL.Data;
using TaskManagement.DAL.Entities;
using TaskManagement.DAL.Interfaces;


namespace TaskManagement.DAL.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskDbContext _context;
        public TaskRepository(TaskDbContext context) => _context = context;
        public IQueryable<TaskItem> GetQueryable() => _context.Tasks.AsNoTracking();

        public async Task<IEnumerable<TaskItem>> GetAll() => await _context.Tasks.AsNoTracking().ToListAsync();

        public async Task<TaskItem?> GetById(int id) =>  await _context.Tasks.FindAsync(id);

        public async Task Add(TaskItem task) => await _context.Tasks.AddAsync(task);

        public void Update(TaskItem task) => _context.Tasks.Update(task);

        public void Delete(TaskItem task) => _context.Tasks.Remove(task);

        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
    }
}
