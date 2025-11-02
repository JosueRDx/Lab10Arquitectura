using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Tickets.Commands;

public class DeleteTicketCommandHandler : IRequestHandler<DeleteTicketCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTicketCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica para manejar el comando DeleteTicketCommand
    public async Task<bool> Handle(DeleteTicketCommand request, CancellationToken cancellationToken)
    {
        var ticketRepository = _unitOfWork.Repository<Ticket>();
        var responseRepository = _unitOfWork.Repository<Response>();

        // 1. Encontrar el ticket por ID
        var ticket = await ticketRepository.AsQueryable()
            .Include(t => t.Responses) 
            .FirstOrDefaultAsync(t => t.TicketId == request.TicketId, cancellationToken);

        if (ticket is null)
        {
            return false; 
        }

        // 2. Eliminar las respuestas asociadas
        foreach (var response in ticket.Responses.ToList())
        {
            responseRepository.Remove(response);
        }

        // 3. Eliminar el ticket
        ticketRepository.Remove(ticket);
        
        // 4. Guardar cambios
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}