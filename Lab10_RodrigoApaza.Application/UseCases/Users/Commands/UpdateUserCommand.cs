using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Users.Commands;

// Esta solicitud contiene el ID del usuario y el DTO de actualización
public record UpdateUserCommand(Guid UserId, UserUpdateDto Dto) : IRequest<UserDto?>;