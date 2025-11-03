using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Responses.Queries;

public class GetResponsesByTicketQueryHandler : IRequestHandler<GetResponsesByTicketQuery, IEnumerable<ResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetResponsesByTicketQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica para manejar la consulta GetResponsesByTicketQuery
    public async Task<IEnumerable<ResponseDto>> Handle(GetResponsesByTicketQuery request, CancellationToken cancellationToken)
    {
        var responses = await _unitOfWork.Repository<Response>()
            .AsQueryable()
            .Include(r => r.Responder)
            .Where(r => r.TicketId == request.TicketId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return responses.Select(MapToDto);
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