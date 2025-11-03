using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Responses.Queries;

// Necesita el TicketId para buscar las respuestas asociadas a ese ticket
public record GetResponsesByTicketQuery(Guid TicketId) : IRequest<IEnumerable<ResponseDto>>;