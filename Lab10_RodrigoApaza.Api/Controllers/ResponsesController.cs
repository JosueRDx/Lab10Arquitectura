using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.UseCases.Responses.Commands; 
using Lab10_RodrigoApaza.Application.UseCases.Responses.Queries; 
using Lab10_RodrigoApaza.Application.UseCases.Tickets.Queries; 
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab10_RodrigoApaza.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ResponsesController : ControllerBase
{
    // Ahora usamos Mediator en lugar de IResponseService y ITicketService
    private readonly IMediator _mediator;

    public ResponsesController(IMediator mediator)
    {
        // Ahora inyectamos IMediator
        _mediator = mediator;
    }

    [HttpGet("ticket/{ticketId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByTicket(Guid ticketId)
    {
        // 1. Validación de permiso usando MediatR
        var ticket = await _mediator.Send(new GetTicketByIdQuery(ticketId));
        
        if (ticket is null)
        {
            return NotFound();
        }

        if (!User.IsInRole("admin") && !User.IsInRole("support") && ticket.UserId != GetCurrentUserId())
        {
            return Forbid();
        }

        // 2. Obtención de las respuestas usando MediatR
        var responses = await _mediator.Send(new GetResponsesByTicketQuery(ticketId));
        return Ok(responses);
    }

    [HttpPost]
    [Authorize(Roles = "admin,support,client")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] ResponseCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1. Validación de permiso usando MediatR
        var ticket = await _mediator.Send(new GetTicketByIdQuery(dto.TicketId));
        
        if (ticket is null)
        {
            return NotFound(new { message = "El ticket especificado no existe." });
        }

        var currentUserId = GetCurrentUserId();
        if (User.IsInRole("client") && ticket.UserId != currentUserId)
        {
            return Forbid();
        }

        try
        {
            // 2. Creación de la respuesta usando MediatR
            var command = new CreateResponseCommand(currentUserId, dto); 
            var response = await _mediator.Send(command);
            
            return CreatedAtAction(nameof(GetByTicket), new { ticketId = dto.TicketId }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        // usamos un comando para eliminar la respuesta
        var command = new DeleteResponseCommand(id);
        var deleted = await _mediator.Send(command);
        
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
    
    // Este método auxiliar obtiene el ID del usuario actual desde los claims
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                          User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : throw new InvalidOperationException("No se pudo determinar el usuario actual.");
    }
}