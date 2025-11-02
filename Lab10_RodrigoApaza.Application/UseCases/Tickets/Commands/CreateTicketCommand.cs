using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Commands;

// Necesita el UserId y el TicketCreateDto para crear un nuevo ticket
public record CreateTicketCommand(Guid UserId, TicketCreateDto Dto) : IRequest<TicketDto>;