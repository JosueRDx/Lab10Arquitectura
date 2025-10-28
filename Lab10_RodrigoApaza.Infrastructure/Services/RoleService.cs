using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        var roles = await _unitOfWork.Repository<Role>()
            .AsQueryable()
            .OrderBy(r => r.RoleName)
            .ToListAsync();

        return roles.Select(MapToDto);
    }

    public async Task<RoleDto?> GetByIdAsync(Guid id)
    {
        var role = await _unitOfWork.Repository<Role>()
            .AsQueryable()
            .FirstOrDefaultAsync(r => r.RoleId == id);

        return role is null ? null : MapToDto(role);
    }

    public async Task<RoleDto> CreateAsync(RoleCreateDto dto)
    {
        var normalizedName = dto.RoleName.Trim();

        var roleRepository = _unitOfWork.Repository<Role>();
        var roleExists = await roleRepository.AsQueryable()
            .AnyAsync(r => r.RoleName.ToLowerInvariant() == normalizedName.ToLowerInvariant());

        if (roleExists)
        {
            throw new InvalidOperationException($"El rol '{normalizedName}' ya existe.");
        }

        var role = new Role
        {
            RoleId = Guid.NewGuid(),
            RoleName = normalizedName
        };

        await roleRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(role);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        var role = await roleRepository.AsQueryable()
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.RoleId == id);

        if (role is null)
        {
            return false;
        }

        var userRoleRepository = _unitOfWork.Repository<UserRole>();
        foreach (var userRole in role.UserRoles.ToList())
        {
            userRoleRepository.Remove(userRole);
        }

        role.UserRoles.Clear();

        roleRepository.Remove(role);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static RoleDto MapToDto(Role role) => new()
    {
        RoleId = role.RoleId,
        RoleName = role.RoleName
    };
}


