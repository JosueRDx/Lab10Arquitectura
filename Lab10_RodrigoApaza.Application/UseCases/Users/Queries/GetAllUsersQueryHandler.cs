using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Users.Queries;

// Este es el manejador para GetAllUsersQuery
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // logica de negocio movida desde UserService.cs
    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        // Esta lógica obtiene todos los usuarios con sus roles
        var users = await _unitOfWork.Repository<User>()
            .AsQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync(cancellationToken);

        // Usamos el método de mapeo para convertir a DTOs
        return users.Select(MapToDto);
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