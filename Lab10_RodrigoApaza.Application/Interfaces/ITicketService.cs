using Lab10_RodrigoApaza.Application.DTOs;

namespace Lab10_RodrigoApaza.Application.Interfaces;

public interface ITicketService
{
    Task<IEnumerable<TicketDto>> GetAllAsync();
    Task<TicketDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TicketDto>> GetByUserAsync(Guid userId);
    Task<TicketDto> CreateAsync(Guid userId, TicketCreateDto dto);
    Task<TicketDto?> UpdateAsync(Guid id, TicketUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}