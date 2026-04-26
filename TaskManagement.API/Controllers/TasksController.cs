using Microsoft.AspNetCore.Mvc;
using TaskManagement.BLL.DTOs;
using TaskManagement.BLL.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;
    public TasksController(ITaskService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TaskFilterOptions filterOptions)
    {
        var response = await _service.GetAllTasks(filterOptions);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _service.GetTaskById(id);
        return task == null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TaskCreateDto dto)
    {
        var result = await _service.CreateTask(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TaskCreateDto dto)
    {
        var success = await _service.UpdateTask(id, dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteTask(id);
        return success ? NoContent() : NotFound();
    }
}