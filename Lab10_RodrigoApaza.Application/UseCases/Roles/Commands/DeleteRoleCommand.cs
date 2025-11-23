using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Roles.Commands;

// Esta solicitud contiene el Id del rol a eliminar
public record DeleteRoleCommand(Guid RoleId) : IRequest<bool>;