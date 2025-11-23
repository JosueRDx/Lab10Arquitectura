using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Roles.Queries;

// No tenemos parámetros, y usamos 'record' por ser una clase inmutable de solo datos.
public record GetAllRolesQuery() : IRequest<IEnumerable<RoleDto>>;