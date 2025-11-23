using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Roles.Commands;

// Esta solicitud contiene el DTO de creación
public record CreateRoleCommand(RoleCreateDto Dto) : IRequest<RoleDto>;