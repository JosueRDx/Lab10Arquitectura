using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Queries;

// No tiene parámetros, porque solo queremos todos los tickets
public record GetAllTicketsQuery() : IRequest<IEnumerable<TicketDto>>;