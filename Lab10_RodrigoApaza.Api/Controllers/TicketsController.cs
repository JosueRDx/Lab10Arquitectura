using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.UseCases.Tickets.Commands;
using Lab10_RodrigoApaza.Application.UseCases.Tickets.Queries; 
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab10_RodrigoApaza.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TicketsController : ControllerBase
{
    // Ahora usamos Mediator en lugar de ITicketService
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        // Ahora inyectamos IMediator
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "admin,support")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        // Usamos una consulta para obtener todos los tickets
        var query = new GetAllTicketsQuery();
        var tickets = await _mediator.Send(query);
        return Ok(tickets);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "client,admin")]
    [ProducesResponseType(typeof(IEnumerable<TicketDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTickets()
    {
        // Usamos una consulta para obtener los tickets del usuario actual
        var userId = GetCurrentUserId(); 
        var query = new GetMyTicketsQuery(userId); 
        var tickets = await _mediator.Send(query);
        return Ok(tickets);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Usamos una consulta para obtener el ticket por ID
        var query = new GetTicketByIdQuery(id);
        var ticket = await _mediator.Send(query);
        
        if (ticket is null)
        {
            return NotFound();
        }

        // Lógica de autorización
        if (User.IsInRole("admin") || User.IsInRole("support") || ticket.UserId == GetCurrentUserId())
        {
            return Ok(ticket);
        }

        return Forbid();
    }

    [HttpPost]
    [Authorize(Roles = "client,admin")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] TicketCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        try
        {
            // Usamos un comando para crear el ticket
            var command = new CreateTicketCommand(userId, dto);
            var ticket = await _mediator.Send(command);
            
            return CreatedAtAction(nameof(GetById), new { id = ticket.TicketId }, ticket);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin,support")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] TicketUpdateDto dto)
    {
        // Usamos un comando para actualizar el ticket
        var command = new UpdateTicketCommand(id, dto);
        var ticket = await _mediator.Send(command);
        
        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)    
    {
        // Usamos un comando para eliminar el ticket
        var command = new DeleteTicketCommand(id);
        var deleted = await _mediator.Send(command);

        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
    
    // Este método obtiene el ID del usuario actual desde los claims
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                          User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : throw new InvalidOperationException("No se pudo determinar el usuario actual.");
    }
}