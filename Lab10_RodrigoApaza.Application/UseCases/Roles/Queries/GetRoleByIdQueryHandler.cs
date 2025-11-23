using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Roles.Queries;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica de negocio movida desde RoleService.cs
    public async Task<RoleDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Repository<Role>()
            .AsQueryable()
            .FirstOrDefaultAsync(r => r.RoleId == request.RoleId, cancellationToken);

        return role is null ? null : MapToDto(role);
    }

    // Método de mapeo copiado de RoleService.cs
    private static RoleDto MapToDto(Role role) => new()
    {
        RoleId = role.RoleId,
        RoleName = role.RoleName
    };
}