using Lab10_RodrigoApaza.Application.DTOs;

namespace Lab10_RodrigoApaza.Application.Interfaces;

public interface IResponseService
{
    Task<IEnumerable<ResponseDto>> GetByTicketAsync(Guid ticketId);
    Task<ResponseDto?> GetByIdAsync(Guid responseId);
    Task<ResponseDto> CreateAsync(Guid responderId, ResponseCreateDto dto);
    Task<bool> DeleteAsync(Guid responseId);
}