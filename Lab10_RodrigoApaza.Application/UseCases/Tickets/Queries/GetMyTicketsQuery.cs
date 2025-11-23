using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Queries;

// Necesita el UserId para buscar los tickets asociados a ese usuario
public record GetMyTicketsQuery(Guid UserId) : IRequest<IEnumerable<TicketDto>>;