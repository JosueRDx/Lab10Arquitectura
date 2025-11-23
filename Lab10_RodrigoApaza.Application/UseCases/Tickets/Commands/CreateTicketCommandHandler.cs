using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Commands;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, TicketDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTicketCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica para manejar el comando CreateTicketCommand
    public async Task<TicketDto> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        // 1. Validación
        if (string.IsNullOrWhiteSpace(request.Dto.Title))
        {
            throw new ArgumentException("El título del ticket es obligatorio.", nameof(request.Dto.Title));
        }

        // 2. Creación del Ticket
        var ticket = new Ticket
        {
            TicketId = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Dto.Title.Trim(),
            Description = request.Dto.Description,
            Status = "open",
            CreatedAt = DateTime.UtcNow
        };

        // 3. Guardar en la db
        await _unitOfWork.Repository<Ticket>().AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        // 4. Obtener y devolver el DTO completo del ticket creado
        var createdTicket = await BuildTicketQuery()
            .FirstAsync(t => t.TicketId == ticket.TicketId, cancellationToken);
        
        return MapToDto(createdTicket);
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