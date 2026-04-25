using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TaskManagement.API.Middleware;
using TaskManagement.BLL.Interfaces;
using TaskManagement.BLL.Profiles;
using TaskManagement.BLL.Services;
using TaskManagement.DAL.Data;
using TaskManagement.DAL.Interfaces;
using TaskManagement.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();



builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
}, typeof(MappingProfile).Assembly);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();