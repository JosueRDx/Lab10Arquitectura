using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.UseCases.Roles.Commands;
using Lab10_RodrigoApaza.Application.UseCases.Roles.Queries; 
using MediatR; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab10_RodrigoApaza.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin")]
public class RolesController : ControllerBase
{
    // Ahora usamos Mediator en lugar de IRoleService
    private readonly IMediator _mediator; 

    public RolesController(IMediator mediator)
    {
        // Ahora inyectamos IMediator
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        // Usamos una consulta para obtener todos los roles
        var query = new GetAllRolesQuery();
        var roles = await _mediator.Send(query);
        return Ok(roles);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Usamos una consulta para obtener el rol por ID
        var query = new GetRoleByIdQuery(id);
        var role = await _mediator.Send(query);
        
        if (role is null)
        {
            return NotFound();
        }
        return Ok(role);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] RoleCreateDto dto)    
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Usamos un comando para crear el rol
            var command = new CreateRoleCommand(dto);
            var role = await _mediator.Send(command);
            
            return CreatedAtAction(nameof(GetById), new { id = role.RoleId }, role);
        }
        catch (InvalidOperationException ex) 
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        // Usamos un comando para eliminar el rol
        var command = new DeleteRoleCommand(id);
        var deleted = await _mediator.Send(command);
        
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}