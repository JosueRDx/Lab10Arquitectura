using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Users.Queries;

// Manejador de la consulta GetUserByIdQuery
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    // Inyección de dependencias
    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // La lógica de negocio, movida desde UserService.cs
    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>()
            .AsQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        // Devolvemos el DTO mapeado
        return MapToDto(user);
    }
    
    // Copiamos el método de mapeo desde UserService.cs original
    private static UserDto MapToDto(User user) => new()
    {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        CreatedAt = user.CreatedAt,
        Roles = user.UserRoles.Select(ur => ur.Role.RoleName)
    };
}