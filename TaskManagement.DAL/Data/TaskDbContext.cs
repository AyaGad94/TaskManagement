using Microsoft.EntityFrameworkCore;
using TaskManagement.DAL.Entities;

namespace TaskManagement.DAL.Data;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options) { }
    public DbSet<TaskItem> Tasks { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.Status);

        base.OnModelCreating(modelBuilder);
    }
}