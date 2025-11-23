using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Queries;

public class GetAllTicketsQueryHandler : IRequestHandler<GetAllTicketsQuery, IEnumerable<TicketDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTicketsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica para manejar la consulta
    public async Task<IEnumerable<TicketDto>> Handle(GetAllTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await BuildTicketQuery()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tickets.Select(MapToDto);
    }
    
    // Construye la consulta base para obtener tickets con sus relaciones necesarias
    private IQueryable<Ticket> BuildTicketQuery()
    {
        return _unitOfWork.Repository<Ticket>()
            .AsQueryable()
            .Include(t => t.User) // Incluye el usuario que creó el ticket
            .Include(t => t.Responses) // Incluye las respuestas del ticket
            .ThenInclude(r => r.Responder); // Incluye el usuario que respondió
    }

    private static TicketDto MapToDto(Ticket ticket) => new()
    {   
        // Mapea las propiedades del ticket al DTO
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