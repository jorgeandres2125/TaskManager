using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using TaskManager.Api.Requests;
using TaskManager.Application.Abstractions.Messaging;
using TaskManager.Application.DTOs.Pagination;
using TaskManager.Application.DTOs.Users;
using TaskManager.Application.UseCases.Commands.Users;
using TaskManager.Application.UseCases.Queries.Users;
using TaskManager.Domain.Entities.Users;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Create(
        CreateUserRequest request,
        [FromServices] IRequestHandler<CreateUserCommand, UserDto> handler,
        CancellationToken cancellationToken)
    {
        var user = await handler.HandleAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>Lista usuarios paginados, ordenados por nombre.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResultDto<UserDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<UserDto>>> List(
        [FromQuery] GetUsersRequest request,
        [FromServices] IRequestHandler<GetUsersQuery, PagedResultDto<UserDto>> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(request.ToQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(
        Guid id,
        [FromServices] IRequestHandler<GetUserByIdQuery, UserDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetUserByIdQuery(id), cancellationToken));

    [HttpPut("{id:guid}")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Update(
        Guid id,
        UpdateUserRequest request,
        [FromServices] IRequestHandler<UpdateUserCommand, UserDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(request.ToCommand(id), cancellationToken));

    /// <summary>
    /// Sube o reemplaza la foto del usuario (JPEG, PNG o WebP de hasta 2 MB). Se guarda en la base de datos
    /// (VARBINARY(MAX)) con el nombre del archivo y su extensión. Devuelve el usuario con la versión de la foto.
    /// </summary>
    [HttpPut("{id:guid}/photo")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = PhotoFormLimitBytes)]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UploadPhoto(
        Guid id,
        [FromForm] UploadUserPhotoRequest request,
        [FromServices] IRequestHandler<UploadUserPhotoCommand, UserDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(await request.ToCommandAsync(id, cancellationToken), cancellationToken));

    /// <summary>
    /// Devuelve la foto como imagen, con su nombre de archivo original. Usa ETag y Last-Modified:
    /// si no cambió, responde 304 y el navegador usa la copia que ya tiene.
    /// </summary>
    [HttpGet("{id:guid}/photo")]
    [Produces("image/jpeg", "image/png", "image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> GetPhoto(
        Guid id,
        [FromServices] IRequestHandler<GetUserPhotoQuery, UserPhotoFileDto> handler,
        CancellationToken cancellationToken)
    {
        var photo = await handler.HandleAsync(new GetUserPhotoQuery(id), cancellationToken);

        Response.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline") { FileNameStar = photo.FileName }.ToString();
        Response.Headers.CacheControl = "no-cache";

        return File(
            photo.Content,
            photo.ContentType,
            lastModified: new DateTimeOffset(photo.Version),
            entityTag: new EntityTagHeaderValue($"\"{photo.Version.Ticks}\""));
    }

    /// <summary>Quita la foto del usuario (si no tiene, no es un error).</summary>
    [HttpDelete("{id:guid}/photo")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> DeletePhoto(
        Guid id,
        [FromServices] IRequestHandler<DeleteUserPhotoCommand, UserDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new DeleteUserPhotoCommand(id), cancellationToken));

    private const long PhotoFormLimitBytes = UserPhoto.MaxSizeBytes + 1024 * 1024;
}
