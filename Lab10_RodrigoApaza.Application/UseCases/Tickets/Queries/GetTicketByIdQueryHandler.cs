using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Queries;

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTicketByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica para manejar la consulta
    public async Task<TicketDto?> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await BuildTicketQuery() 
            .FirstOrDefaultAsync(t => t.TicketId == request.TicketId, cancellationToken);

        return ticket is null ? null : MapToDto(ticket); 
    }

    // Construye la consulta base para obtener tickets con sus relaciones necesarias
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