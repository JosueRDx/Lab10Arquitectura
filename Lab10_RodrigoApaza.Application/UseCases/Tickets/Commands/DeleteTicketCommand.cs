using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Commands;

// Necesita el TicketId para eliminar un ticket existente
public record DeleteTicketCommand(Guid TicketId) : IRequest<bool>;