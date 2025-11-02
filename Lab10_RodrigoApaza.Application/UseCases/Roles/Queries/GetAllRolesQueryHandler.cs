using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Roles.Queries;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, IEnumerable<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllRolesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica de negocio movida desde RoleService.cs
    public async Task<IEnumerable<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Repository<Role>()
            .AsQueryable()
            .OrderBy(r => r.RoleName)
            .ToListAsync(cancellationToken);

        return roles.Select(MapToDto); // Usamos el método de mapeo para convertir a DTOs
    }
    
    // Método de mapeo copiado desde RoleService.cs original
    private static RoleDto MapToDto(Role role) => new()
    {
        RoleId = role.RoleId,
        RoleName = role.RoleName
    };
}