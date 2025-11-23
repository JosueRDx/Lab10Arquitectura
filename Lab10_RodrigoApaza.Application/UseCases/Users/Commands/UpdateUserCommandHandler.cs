using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Users.Commands;

// Manejador del comando de actualización de usuario
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica de negocio para actualizar un usuario
    public async Task<UserDto?> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var userRepository = _unitOfWork.Repository<User>();
        
        // 1. Obtener el usuario existente
        var user = await userRepository.AsQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user is null)
        {
            return null; 
        }

        var dto = request.Dto;

        // 2. Lógica de actualización de Email
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            var emailExists = await userRepository.AsQueryable()
                .AnyAsync(u => u.Email == dto.Email && u.UserId != request.UserId, cancellationToken);

            if (emailExists)
            {
                throw new InvalidOperationException($"El correo '{dto.Email}' ya se encuentra registrado.");
            }
            user.Email = dto.Email;
        }

        // 3. Lógica de actualización de Password
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        // 4. Lógica de actualización de Roles
        if (dto.Roles is not null)
        {
            // Usamos el método helper copiado de UserService.cs
            await AssignRolesAsync(user, dto.Roles, cancellationToken);
        }

        // 5. Guardamos los cambios
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();

        // 6. Devolvemos el DTO mapeado
        return MapToDto(user);
    }

   

    // Copiamos el método de asignación de roles desde UserService.cs
    private async Task AssignRolesAsync(User user, IEnumerable<string> roleNames, CancellationToken cancellationToken)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        var userRoleRepository = _unitOfWork.Repository<UserRole>();

        var normalizedRoles = roleNames
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        var availableRoles = await roleRepository.AsQueryable()
            .Where(r => normalizedRoles.Contains(r.RoleName.ToLower()))
            .ToListAsync(cancellationToken);

        if (availableRoles.Count != normalizedRoles.Count)
        {
            var existing = availableRoles.Select(r => r.RoleName.ToLowerInvariant()).ToHashSet();
            var missing = normalizedRoles.Where(r => !existing.Contains(r));
            throw new KeyNotFoundException($"No se encontraron los roles: {string.Join(", ", missing)}");
        }

        // 1. Borra los roles existentes
        foreach (var userRole in user.UserRoles.ToList())
        {
            userRoleRepository.Remove(userRole);
        }
        user.UserRoles.Clear();

        // 2. Añade los nuevos roles
        foreach (var role in availableRoles)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                AssignedAt = DateTime.UtcNow
            });
        }
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