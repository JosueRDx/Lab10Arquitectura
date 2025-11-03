using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Responses.Queries;

public class GetResponseByIdQueryHandler : IRequestHandler<GetResponseByIdQuery, ResponseDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetResponseByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica para manejar la consulta GetResponseByIdQuery
    public async Task<ResponseDto?> Handle(GetResponseByIdQuery request, CancellationToken cancellationToken)
    {
        var response = await _unitOfWork.Repository<Response>()
            .AsQueryable()
            .Include(r => r.Responder)
            .FirstOrDefaultAsync(r => r.ResponseId == request.ResponseId, cancellationToken);

        return response is null ? null : MapToDto(response);
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