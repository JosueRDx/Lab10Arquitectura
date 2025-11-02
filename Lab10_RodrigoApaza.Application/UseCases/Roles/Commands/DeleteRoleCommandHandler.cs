using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Roles.Commands;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica que hace el trabajo de eliminar un rol
    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        
        // 1. Encontrar el rol por Id, incluyendo las relaciones con UserRoles
        var role = await roleRepository.AsQueryable()
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.RoleId == request.RoleId, cancellationToken);

        if (role is null)
        {
            return false;
        }

        // 2. Eliminar las relaciones en UserRoles
        var userRoleRepository = _unitOfWork.Repository<UserRole>();
        foreach (var userRole in role.UserRoles.ToList())
        {
            userRoleRepository.Remove(userRole);
        }
        role.UserRoles.Clear();

        // 3. Eliminar el rol
        roleRepository.Remove(role);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
}