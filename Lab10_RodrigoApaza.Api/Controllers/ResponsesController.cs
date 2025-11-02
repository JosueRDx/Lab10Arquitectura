using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab10_RodrigoApaza.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ResponsesController : ControllerBase
{
    private readonly IResponseService _responseService;
    private readonly ITicketService _ticketService;
    
    public ResponsesController(IResponseService responseService, ITicketService ticketService)
    {
        _responseService = responseService;
        _ticketService = ticketService;
    }

    [HttpGet("ticket/{ticketId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByTicket(Guid ticketId)
    {
        var ticket = await _ticketService.GetByIdAsync(ticketId);
        if (ticket is null)
        {
            return NotFound();
        }

        if (!User.IsInRole("admin") && !User.IsInRole("support") && ticket.UserId != GetCurrentUserId())
        {
            return Forbid();
        }

        var responses = await _responseService.GetByTicketAsync(ticketId);
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

        var ticket = await _ticketService.GetByIdAsync(dto.TicketId);
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
            var response = await _responseService.CreateAsync(currentUserId, dto);
            return CreatedAtAction(nameof(GetByTicket), new { ticketId = dto.TicketId }, response);
        }
        catch (ArgumentException ex)
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
        var deleted = await _responseService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                          User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : throw new InvalidOperationException("No se pudo determinar el usuario actual.");
    }
}


