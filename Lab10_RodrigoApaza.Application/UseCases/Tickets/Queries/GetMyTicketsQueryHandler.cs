using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Queries;

public class GetMyTicketsQueryHandler : IRequestHandler<GetMyTicketsQuery, IEnumerable<TicketDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyTicketsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica para manejar la consulta GetMyTicketsQuery
    public async Task<IEnumerable<TicketDto>> Handle(GetMyTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await BuildTicketQuery()
            .Where(t => t.UserId == request.UserId) 
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tickets.Select(MapToDto);
    }

    // Construye la consulta base para los tickets, incluyendo las relaciones necesarias
    private IQueryable<Ticket> BuildTicketQuery()
    {
        return _unitOfWork.Repository<Ticket>()
            .AsQueryable()
            .Include(t => t.User)
            .Include(t => t.Responses)
            .ThenInclude(r => r.Responder);
    }

    private static TicketDto MapToDto(Ticket ticket) => new()
    {
        // Mapea las propiedades del ticket a TicketDto
        TicketId = ticket.TicketId,
        UserId = ticket.UserId,
        Title = ticket.Title,
        Description = ticket.Description,
        Status = ticket.Status,
        CreatedAt = ticket.CreatedAt,
        ClosedAt = ticket.ClosedAt,
        Username = ticket.User?.Username,
        Responses = ticket.Responses
            .OrderBy(r => r.CreatedAt)
            .Select(r => new ResponseDto
            {
                ResponseId = r.ResponseId,
                TicketId = r.TicketId,
                ResponderId = r.ResponderId,
                Message = r.Message,
                CreatedAt = r.CreatedAt,
                ResponderUsername = r.Responder?.Username
            })
    };
}