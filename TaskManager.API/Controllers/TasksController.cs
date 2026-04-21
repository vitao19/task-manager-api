using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.TaskDtos;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Enums;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _service;

        public TasksController(ITaskService service)
        {
            _service = service;
        }

        #region Read Endpoints

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TaskItemStatus? status, [FromQuery] DateTime? dueDate)
        {
            var tasks = await _service.GetAllAsync(status, dueDate);
            return Ok(tasks); 
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var task = await _service.GetByIdAsync(id);

            if (task == null)
                return NotFound(new { message = "Tarefa não encontrada." }); 

            return Ok(task); 
        }

        #endregion

        #region Write Endpoints

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            var result = await _service.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);

            if (!success)
                return NotFound(new { message = "Tarefa não encontrada para atualização." });

            return NoContent(); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Tarefa não encontrada para exclusão." });

            return NoContent(); 
        }
        #endregion
    }
}
