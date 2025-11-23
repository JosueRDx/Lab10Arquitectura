using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.UseCases.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab10_RodrigoApaza.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    // Ahora usamos MediatR en lugar de un servicio directo
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        // Ahora inyectamos el mediador
        _mediator = mediator;
    }
    
    // Inicia sesión en el sistema y genera un token JWT.
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        // Usamos el patrón Mediator para enviar el comando de login
        var command = new LoginCommand(loginRequest);
        var loginResponse = await _mediator.Send(command);

        if (loginResponse == null)
        {
            return Unauthorized(new { message = "Credenciales inválidas" });
        }
        return Ok(loginResponse);
    } 
}