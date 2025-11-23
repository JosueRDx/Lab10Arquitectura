using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Users.Queries;

// Usamos 'record' por ser una clase inmutable de solo datos.
public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;