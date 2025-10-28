using Lab10_RodrigoApaza.Application.DTOs;

namespace Lab10_RodrigoApaza.Application.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllAsync();
    Task<RoleDto?> GetByIdAsync(Guid id);
    Task<RoleDto> CreateAsync(RoleCreateDto dto);
    Task<bool> DeleteAsync(Guid id);
}