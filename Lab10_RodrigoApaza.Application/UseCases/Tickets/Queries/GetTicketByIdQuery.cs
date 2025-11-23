using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Queries;

// Necesita el TicketId para buscar el ticket específico
public record GetTicketByIdQuery(Guid TicketId) : IRequest<TicketDto?>;