using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Infrastructure.Services;

public class ResponseService : IResponseService
{
    private readonly IUnitOfWork _unitOfWork;

    public ResponseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ResponseDto>> GetByTicketAsync(Guid ticketId)
    {
        var responses = await _unitOfWork.Repository<Response>()
            .AsQueryable()
            .Include(r => r.Responder)
            .Where(r => r.TicketId == ticketId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        return responses.Select(MapToDto);
    }

    public async Task<ResponseDto?> GetByIdAsync(Guid responseId)
    {
        var response = await _unitOfWork.Repository<Response>()
            .AsQueryable()
            .Include(r => r.Responder)
            .FirstOrDefaultAsync(r => r.ResponseId == responseId);

        return response is null ? null : MapToDto(response);
    }

    public async Task<ResponseDto> CreateAsync(Guid responderId, ResponseCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Message))
        {
            throw new ArgumentException("El mensaje de la respuesta es obligatorio.", nameof(dto.Message));
        }

        var ticketExists = await _unitOfWork.Repository<Ticket>()
            .AsQueryable()
            .AnyAsync(t => t.TicketId == dto.TicketId);

        if (!ticketExists)
        {
            throw new KeyNotFoundException("El ticket especificado no existe.");
        }

        var responderExists = await _unitOfWork.Repository<User>()
            .AsQueryable()
            .AnyAsync(u => u.UserId == responderId);

        if (!responderExists)
        {
            throw new KeyNotFoundException("El usuario que responde no existe.");
        }

        var response = new Response
        {
            ResponseId = Guid.NewGuid(),
            TicketId = dto.TicketId,
            ResponderId = responderId,
            Message = dto.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Response>().AddAsync(response);
        await _unitOfWork.SaveChangesAsync();

        return (await GetByIdAsync(response.ResponseId))!;
    }

    public async Task<bool> DeleteAsync(Guid responseId)
    {
        var responseRepository = _unitOfWork.Repository<Response>();
        var response = await responseRepository.AsQueryable()
            .FirstOrDefaultAsync(r => r.ResponseId == responseId);

        if (response is null)
        {
            return false;
        }

        responseRepository.Remove(response);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

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