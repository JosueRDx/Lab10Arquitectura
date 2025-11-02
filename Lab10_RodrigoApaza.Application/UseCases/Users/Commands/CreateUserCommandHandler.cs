using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Users.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Lógica de creación de usuario
        var dto = request.Dto;
        var normalizedUsername = dto.Username.Trim();
        var userRepository = _unitOfWork.Repository<User>();

        var usernameExists = await userRepository.AsQueryable()
            .AnyAsync(u => u.Username == normalizedUsername, cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException($"El usuario '{normalizedUsername}' ya existe.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailExists = await userRepository.AsQueryable()
                .AnyAsync(u => u.Email == dto.Email, cancellationToken);
            if (emailExists)
            {
                throw new InvalidOperationException($"El correo '{dto.Email}' ya se encuentra registrado.");
            }
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("La contraseña es obligatoria.", nameof(dto.Password));
        }

        // Crear la entidad User
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = normalizedUsername,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        // Asignar roles si se proporcionan
        var roleNames = dto.Roles ?? Enumerable.Empty<string>();
        var availableRoles = await AssignRolesToUserAsync(user, roleNames, cancellationToken);
        
        await userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        
        // Devolvemos el DTO mapeado
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Roles = availableRoles.Select(r => r.RoleName) 
        };
    }

    // Lo hacemos privado aquí dentro del handler
    private async Task<List<Role>> AssignRolesToUserAsync(User user, IEnumerable<string> roleNames, CancellationToken cancellationToken)
    {
        var roleRepository = _unitOfWork.Repository<Role>();

        var normalizedRoles = roleNames
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (!normalizedRoles.Any())
        {
            return new List<Role>(); 
        }

        var availableRoles = await roleRepository.AsQueryable()
            .Where(r => normalizedRoles.Contains(r.RoleName.ToLowerInvariant()))
            .ToListAsync(cancellationToken);

        if (availableRoles.Count != normalizedRoles.Count)
        {
            var existing = availableRoles.Select(r => r.RoleName.ToLowerInvariant()).ToHashSet();
            var missing = normalizedRoles.Where(r => !existing.Contains(r));
            throw new KeyNotFoundException($"No se encontraron los roles: {string.Join(", ", missing)}");
        }
        
        // Esta lógica es para crear. 
        user.UserRoles.Clear();
        foreach (var role in availableRoles)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                AssignedAt = DateTime.UtcNow
            });
        }
        
        return availableRoles;
    }
}