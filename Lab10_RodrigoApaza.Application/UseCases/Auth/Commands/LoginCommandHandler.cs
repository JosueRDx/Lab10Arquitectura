using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Lab10_RodrigoApaza.Application.UseCases.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    // Lógica de autenticación y generación de JWT
    public async Task<LoginResponseDto?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = request.LoginRequest;

        // 1. Encontrar el usuario por nombre de usuario
        var user = await _unitOfWork.Repository<User>()
            .AsQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == loginRequest.Username, cancellationToken);

        if (user is null)
        {
            return null;
        }

        // 2. Verificar la contraseña
        var isPasswordValid = user.PasswordHash == loginRequest.Password;
        if (!isPasswordValid)
        {
            try
            {
                // Intenta verificar usando BCrypt
                isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                isPasswordValid = false;
            }
        }

        if (!isPasswordValid)
        {
            return null;
        }

        // 3. Obtener roles del usuario
        var roles = user.UserRoles
            .Select(ur => ur.Role.RoleName)
            .Distinct()
            .ToList();

        // 4. Crear Claims para el JWT
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // 5. Leer configuración de JWT y crear el token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(2);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = credentials
        };

        // 6. Crear y escribir el token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // 7. Devolver la respuesta con el token y la información del usuario
        return new LoginResponseDto
        {
            Token = tokenHandler.WriteToken(token),
            ExpiresAt = expires,
            Username = user.Username,
            Roles = roles
        };
    }
}