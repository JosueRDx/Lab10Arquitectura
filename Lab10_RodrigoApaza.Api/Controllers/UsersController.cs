using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.UseCases.Users.Commands;
using Lab10_RodrigoApaza.Application.UseCases.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab10_RodrigoApaza.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "admin")]
public class UsersController : ControllerBase
{
    // Ahora solo dependemos de MediatR
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    // Obtiene todos los usuarios migrado a CQRS.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        // 1. Crea la consulta
        var query = new GetAllUsersQuery();
        
        // 2. Envía la consulta a MediatR
        var users = await _mediator.Send(query);
        
        return Ok(users);
    }
    
    // Obtiene un usuario por su ID migrado a CQRS.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var user = await _mediator.Send(query);
        
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
    
    // Crea un nuevo usuario migrado a CQRS.
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // 1. Crea el comando
            var command = new CreateUserCommand(dto);
            
            // 2. Envía el comando a MediatR
            var user = await _mediator.Send(command);
            
            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
        }
        catch (InvalidOperationException ex) 
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex) 
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex) 
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    // Actualiza un usuario migrado a CQRS.
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // 1. Crea el comando, pasando el ID de la ruta y el DTO del body
            var command = new UpdateUserCommand(id, dto);
            
            // 2. Envía el comando a MediatR
            var user = await _mediator.Send(command);
            
            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (InvalidOperationException ex) 
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex) 
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    // Elimina un usuario migrado a CQRS.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        // 1. Crea el comando
        var command = new DeleteUserCommand(id);
        
        // 2. Envía el comando a MediatR
        var deleted = await _mediator.Send(command);
        
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}