using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Users.Commands;

// Esta solicitud contiene el ID del usuario a eliminar
public record DeleteUserCommand(Guid UserId) : IRequest<bool>;