using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Users.Commands;

// Esta solicitud contiene el DTO de creación
public record CreateUserCommand(UserCreateDto Dto) : IRequest<UserDto>;