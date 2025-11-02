using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Roles.Queries;

// Esta solicitud contiene el Id del rol y se usa record por ser inmutable
public record GetRoleByIdQuery(Guid RoleId) : IRequest<RoleDto?>;