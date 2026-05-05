using Moq;
using FluentAssertions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskManagement.BLL.Services;
using TaskManagement.BLL.Profiles;
using TaskManagement.BLL.Exceptions;
using TaskManagement.DAL.Interfaces;
using TaskManagement.DAL.Entities;
using TaskManagement.BLL.DTOs;
using TaskManagement.DAL.Enums;
using MockQueryable.Moq;

namespace TaskManagement.Tests.Unit;
public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _mockRepo;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<TaskService>> _mockLogger;
    private readonly TaskService _service;

    public  TaskServiceTests()
    {
        _mockRepo = new Mock<ITaskRepository>();
        _mockLogger = new Mock<ILogger<TaskService>>();
        _mapper = new Mapper(new MapperConfiguration(cfg =>
    cfg.AddProfile(new MappingProfile())));
        _service = new TaskService(_mockRepo.Object, _mapper, _mockLogger.Object);
    }

    // ─── CreateTask ───────────────────────────────────────────────

    [Fact]
    public async Task CreateTask_ShouldReturnTaskDto_WhenValidInput()
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = "Test Task",
            Description = "Test Description",
            Status = UserTaskStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(1)
        };
        _mockRepo.Setup(r => r.Add(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.CreateTask(dto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task CreateTask_ShouldCallRepository_Once()
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = "Test Task",
            Status = UserTaskStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(1)
        };
        _mockRepo.Setup(r => r.Add(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        await _service.CreateTask(dto);

        // Assert
        _mockRepo.Verify(r => r.Add(It.IsAny<TaskItem>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ─── GetTaskById ──────────────────────────────────────────────

    [Fact]
    public async Task GetTaskById_ShouldReturnTask_WhenTaskExists()
    {
        // Arrange
        var testTask = new TaskItem
        {
            Id = 1,
            Title = "Test Task",
            Status = UserTaskStatus.Pending,
            DueDate = DateTime.UtcNow
        };
        var tasks = new List<TaskItem> { testTask }.AsQueryable().BuildMock();
        _mockRepo.Setup(r => r.GetQueryable()).Returns(tasks);

        // Act
        var result = await _service.GetTaskById(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        // Arrange
        var tasks = new List<TaskItem>().AsQueryable().BuildMock();
        _mockRepo.Setup(r => r.GetQueryable()).Returns(tasks);

        // Act
        var result = await _service.GetTaskById(999);

        // Assert
        result.Should().BeNull();
    }

    // ─── DeleteTask ───────────────────────────────────────────────

    [Fact]
    public async Task DeleteTask_ShouldReturnFalse_WhenTaskNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(999)).ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _service.DeleteTask(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteTask_ShouldReturnTrue_WhenTaskExists()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = 1,
            Title = "Task",
            Status = UserTaskStatus.Pending,
            DueDate = DateTime.UtcNow
        };
        _mockRepo.Setup(r => r.GetById(1)).ReturnsAsync(task);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteTask(1);

        // Assert
        result.Should().BeTrue();
        _mockRepo.Verify(r => r.Delete(task), Times.Once);
    }

    // ─── UpdateTask ───────────────────────────────────────────────

    [Fact]
    public async Task UpdateTask_ShouldReturnFalse_WhenTaskNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(999)).ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _service.UpdateTask(999, new TaskCreateDto());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTask_ShouldReturnTrue_WhenTaskExists()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = 1,
            Title = "Old Title",
            Status = UserTaskStatus.Pending,
            DueDate = DateTime.UtcNow
        };
        var dto = new TaskCreateDto
        {
            Title = "New Title",
            Status = UserTaskStatus.Completed,
            DueDate = DateTime.UtcNow
        };
        _mockRepo.Setup(r => r.GetById(1)).ReturnsAsync(task);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateTask(1, dto);

        // Assert
        result.Should().BeTrue();
    }

    // ─── GetAllTasks Validation ───────────────────────────────────

    [Fact]
    public async Task GetAllTasks_ShouldThrowBadRequestException_WhenFutureDateFilter()
    {
        // Arrange
        var tasks = new List<TaskItem>().AsQueryable().BuildMock();
        _mockRepo.Setup(r => r.GetQueryable()).Returns(tasks);

        var filterOptions = new TaskFilterOptions
        {
            DueDateFilter = DateTime.UtcNow.AddDays(5)
        };

        // Act
        var act = async () => await _service.GetAllTasks(filterOptions);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Filtering by a future date is not allowed.");
    }
}