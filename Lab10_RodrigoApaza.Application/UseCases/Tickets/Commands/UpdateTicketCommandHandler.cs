using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Commands;

public class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, TicketDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTicketCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica para manejar el comando UpdateTicketCommand
    public async Task<TicketDto?> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticketRepository = _unitOfWork.Repository<Ticket>();
        
        // 1. Encontrar el ticket por ID 
        var ticket = await ticketRepository.AsQueryable()
            .FirstOrDefaultAsync(t => t.TicketId == request.TicketId, cancellationToken);

        if (ticket is null)
        {
            return null; 
        }

        var dto = request.Dto;

        // 2. Lógica de actualización
        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            ticket.Title = dto.Title.Trim();
        }

        if (dto.Description is not null)
        {
            ticket.Description = dto.Description;
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            var normalizedStatus = dto.Status.Trim();
            ticket.Status = normalizedStatus;

            // Lógica para cerrar el ticket 
            if (string.Equals(normalizedStatus, "closed", StringComparison.OrdinalIgnoreCase) && ticket.ClosedAt is null)
            {
                ticket.ClosedAt = DateTime.UtcNow;
            }
            else if (!string.Equals(normalizedStatus, "closed", StringComparison.OrdinalIgnoreCase))
            {
                ticket.ClosedAt = null;
            }
        }

        if (dto.ClosedAt.HasValue)
        {
            ticket.ClosedAt = dto.ClosedAt;
        }

        // 3. Guardar cambios en la db
        ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        // 4. Obtener y devolver el DTO completo del ticket actualizado
        var updatedTicket = await BuildTicketQuery()
            .FirstAsync(t => t.TicketId == ticket.TicketId, cancellationToken);

        return MapToDto(updatedTicket);
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