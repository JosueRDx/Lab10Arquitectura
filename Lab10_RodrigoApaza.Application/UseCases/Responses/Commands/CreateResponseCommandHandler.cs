using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Responses.Commands;

public class CreateResponseCommandHandler : IRequestHandler<CreateResponseCommand, ResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateResponseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica para manejar el comando CreateResponseCommand
    public async Task<ResponseDto> Handle(CreateResponseCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // 1. Validación de Mensaje
        if (string.IsNullOrWhiteSpace(dto.Message))
        {
            throw new ArgumentException("El mensaje de la respuesta es obligatorio.", nameof(dto.Message));
        }

        // 2. Validación de Ticket 
        var ticketExists = await _unitOfWork.Repository<Ticket>()
            .AsQueryable()
            .AnyAsync(t => t.TicketId == dto.TicketId, cancellationToken);

        if (!ticketExists)
        {
            throw new KeyNotFoundException("El ticket especificado no existe.");
        }

        // 3. Validación de Usuario (Responder)
        var responder = await _unitOfWork.Repository<User>()
            .AsQueryable()
            .FirstOrDefaultAsync(u => u.UserId == request.ResponderId, cancellationToken);

        if (responder is null)
        {
            throw new KeyNotFoundException("El usuario que responde no existe.");
        }

        // 4. Creación de la entidad
        var response = new Response
        {
            ResponseId = Guid.NewGuid(),
            TicketId = dto.TicketId,
            ResponderId = request.ResponderId,
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        // 5. Guardar
        await _unitOfWork.Repository<Response>().AddAsync(response);
        await _unitOfWork.SaveChangesAsync();

        // 6. Cargar el Responder para el mapeo a DTO
        response.Responder = responder;
        return MapToDto(response);
    }
    
    // Mapea la entidad Response a ResponseDto, incluyendo el nombre de usuario del respondedor
    private static ResponseDto MapToDto(Response response) => new()
    {
        ResponseId = response.ResponseId,
        TicketId = response.TicketId,
        ResponderId = response.ResponderId,
        Message = response.Message,
        CreatedAt = response.CreatedAt,
        ResponderUsername = response.Responder?.Username
    };
}