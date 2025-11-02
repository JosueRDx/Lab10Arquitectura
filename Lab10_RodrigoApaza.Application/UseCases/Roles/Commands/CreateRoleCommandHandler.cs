using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Roles.Commands;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica que hace el trabajo de crear un rol
    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Dto.RoleName.Trim();
        var roleRepository = _unitOfWork.Repository<Role>();

        // 1. Validar que no exista otro rol con el mismo nombre
        var roleExists = await roleRepository.AsQueryable()
            .AnyAsync(r => r.RoleName.ToLowerInvariant() == normalizedName.ToLowerInvariant(), cancellationToken);

        if (roleExists)
        {
            // Esta excepción será capturada en el controlador para devolver un 400 Bad Request
            throw new InvalidOperationException($"El rol '{normalizedName}' ya existe.");
        }

        // 2. Crear la entidad Role
        var role = new Role
        {
            RoleId = Guid.NewGuid(),
            RoleName = normalizedName
        };

        // 3. Guardar en la BD
        await roleRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        // 4. Devolver el RoleDto
        return MapToDto(role);
    }
    
    // Método auxiliar para mapear Role a RoleDto
    private static RoleDto MapToDto(Role role) => new()
    {
        RoleId = role.RoleId,
        RoleName = role.RoleName
    };
}