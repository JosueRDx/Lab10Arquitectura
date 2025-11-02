using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Users.Commands;

// Implementa el manejador para DeleteUserCommand
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica de negocio para eliminar un usuario
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var userRepository = _unitOfWork.Repository<User>();
        
        // 1. Encontrar al usuario por su ID, incluyendo sus UserRoles
        var user = await userRepository.AsQueryable()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

        if (user is null)
        {
            return false; 
        }

        // 2. Borrar las asociaciones de UserRoles
        var userRoleRepository = _unitOfWork.Repository<UserRole>();
        foreach (var userRole in user.UserRoles.ToList())
        {
            userRoleRepository.Remove(userRole);
        }
        user.UserRoles.Clear(); 

        // 3. Borrar el usuario
        userRepository.Remove(user);
        
        // 4. Guardar los cambios en la base de datos
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}