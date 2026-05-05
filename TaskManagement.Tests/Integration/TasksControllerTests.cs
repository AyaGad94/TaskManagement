using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.API.Controllers;
using TaskManagement.BLL.DTOs;
using TaskManagement.BLL.Profiles;
using TaskManagement.BLL.Services;
using TaskManagement.DAL.Data;
using TaskManagement.DAL.Entities;
using TaskManagement.DAL.Enums;
using TaskManagement.DAL.Repositories;

namespace TaskManagement.Tests.Integration;

public class TasksControllerTests
{
    private readonly TaskDbContext _context;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskDbContext(options);

        var mapper = new Mapper(new MapperConfiguration(cfg =>
            cfg.AddProfile(new MappingProfile())));

        var logger = Mock.Of<ILogger<TaskService>>();
        var repository = new TaskRepository(_context);
        var service = new TaskService(repository, mapper, logger);

        _controller = new TasksController(service);
    }

    // ─── GetAll ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithPagedResponse()
    {
        // Arrange
        _context.Tasks.Add(new TaskItem
        {
            Title = "Test Task",
            Status = UserTaskStatus.Pending,
            DueDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll(new TaskFilterOptions());

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResponse<TaskDto>>().Subject;
        response.Items.Should().HaveCount(1);
        response.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyList_WhenNoTasks()
    {
        // Act
        var result = await _controller.GetAll(new TaskFilterOptions());

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResponse<TaskDto>>().Subject;
        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldFilterByStatus_Correctly()
    {
        // Arrange
        _context.Tasks.AddRange(
            new TaskItem
            {
                Title = "Pending Task",
                Status = UserTaskStatus.Pending,
                DueDate = DateTime.UtcNow
            },
            new TaskItem
            {
                Title = "Completed Task",
                Status = UserTaskStatus.Completed,
                DueDate = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        var filter = new TaskFilterOptions { StatusFilter = UserTaskStatus.Completed };

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResponse<TaskDto>>().Subject;
        response.Items.Should().HaveCount(1);
        response.Items.First().Title.Should().Be("Completed Task");
    }

    [Fact]
    public async Task GetAll_ShouldRespectPagination()
    {
        // Arrange
        _context.Tasks.AddRange(
            new TaskItem { Title = "Task 1", Status = UserTaskStatus.Pending, DueDate = DateTime.UtcNow },
            new TaskItem { Title = "Task 2", Status = UserTaskStatus.Pending, DueDate = DateTime.UtcNow },
            new TaskItem { Title = "Task 3", Status = UserTaskStatus.Pending, DueDate = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var filter = new TaskFilterOptions { PageNumber = 1, PageSize = 2 };

        // Act
        var result = await _controller.GetAll(filter);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResponse<TaskDto>>().Subject;
        response.Items.Should().HaveCount(2);
        response.TotalCount.Should().Be(3);
        response.TotalPages.Should().Be(2);
    }

    // ─── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenTaskExists()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Existing Task",
            Status = UserTaskStatus.Pending,
            DueDate = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetById(task.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<TaskDto>().Subject;
        dto.Title.Should().Be("Existing Task");
    }

    // ─── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturnCreated_WithNewTask()
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = "New Task",
            Description = "Description",
            Status = UserTaskStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var task = createdResult.Value.Should().BeOfType<TaskDto>().Subject;
        task.Title.Should().Be("New Task");
    }

    [Fact]
    public async Task Create_ShouldPersistTask_InDatabase()
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = "Persisted Task",
            Status = UserTaskStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        await _controller.Create(dto);

        // Assert
        _context.Tasks.Should().HaveCount(1);
        _context.Tasks.First().Title.Should().Be("Persisted Task");
    }

    // ─── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenTaskExists()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Old Title",
            Status = UserTaskStatus.Pending,
            DueDate = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var dto = new TaskCreateDto
        {
            Title = "New Title",
            Status = UserTaskStatus.Completed,
            DueDate = DateTime.UtcNow
        };

        // Act
        var result = await _controller.Update(task.Id, dto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _controller.Update(999, new TaskCreateDto());

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    // ─── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenTaskExists()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Task to Delete",
            Status = UserTaskStatus.Pending,
            DueDate = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Delete(task.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _context.Tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}