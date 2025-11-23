using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Commands;

// Necesita el TicketId y el TicketUpdateDto para actualizar un ticket existente
public record UpdateTicketCommand(Guid TicketId, TicketUpdateDto Dto) : IRequest<TicketDto?>;