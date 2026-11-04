using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Requests;
using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.DTOs.Pagination;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.UseCases.Commands.Tasks;
using TaskManager.Application.UseCases.Queries.Tasks;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TaskDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> Create(
        CreateTaskRequest request,
        [FromServices] IRequestHandler<CreateTaskCommand, TaskDto> handler,
        CancellationToken cancellationToken)
    {
        var task = await handler.HandleAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    /// <summary>
    /// Lista tareas paginadas, filtrando por usuario, estado, prioridad o etiqueta y ordenadas por fecha de creación.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PagedResultDto<TaskDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<TaskDto>>> List(
        [FromQuery] GetTasksRequest request,
        [FromServices] IRequestHandler<GetTasksQuery, PagedResultDto<TaskDto>> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(request.ToQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TaskDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetById(
        Guid id,
        [FromServices] IRequestHandler<GetTaskByIdQuery, TaskDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetTaskByIdQuery(id), cancellationToken));

    /// <summary>
    /// Edita el título, el usuario asignado (reasignación) y la información adicional, que se reemplaza completa.
    /// El estado no se modifica aquí, sino con PUT /api/tasks/{id}/status.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TaskDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> Update(
        Guid id,
        UpdateTaskRequest request,
        [FromServices] IRequestHandler<UpdateTaskCommand, TaskDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(request.ToCommand(id), cancellationToken));

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType<TaskDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskDto>> ChangeStatus(
        Guid id,
        ChangeTaskStatusRequest request,
        [FromServices] IRequestHandler<ChangeTaskStatusCommand, TaskDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(request.ToCommand(id), cancellationToken));
}
