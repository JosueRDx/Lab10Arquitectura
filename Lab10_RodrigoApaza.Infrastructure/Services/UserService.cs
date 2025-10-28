using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _unitOfWork.Repository<User>()
            .AsQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();

        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Repository<User>()
            .AsQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);

        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _unitOfWork.Repository<User>()
            .AsQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username);

        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(UserCreateDto dto)
    {
        var normalizedUsername = dto.Username.Trim();
        var userRepository = _unitOfWork.Repository<User>();

        var usernameExists = await userRepository.AsQueryable()
            .AnyAsync(u => u.Username == normalizedUsername);

        if (usernameExists)
        {
            throw new InvalidOperationException($"El usuario '{normalizedUsername}' ya existe.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailExists = await userRepository.AsQueryable()
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
            {
                throw new InvalidOperationException($"El correo '{dto.Email}' ya se encuentra registrado.");
            }
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("La contraseña es obligatoria.", nameof(dto.Password));
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = normalizedUsername,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user);

        if (dto.Roles?.Any() == true)
        {
            await AssignRolesAsync(user, dto.Roles);
        }

        await _unitOfWork.SaveChangesAsync();

        return (await GetByIdAsync(user.UserId))!;
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UserUpdateDto dto)
    {
        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.AsQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            var emailExists = await userRepository.AsQueryable()
                .AnyAsync(u => u.Email == dto.Email && u.UserId != id);

            if (emailExists)
            {
                throw new InvalidOperationException($"El correo '{dto.Email}' ya se encuentra registrado.");
            }

            user.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        if (dto.Roles is not null)
        {
            await AssignRolesAsync(user, dto.Roles);
        }

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userRepository = _unitOfWork.Repository<User>();
        var user = await userRepository.AsQueryable()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user is null)
        {
            return false;
        }

        var userRoleRepository = _unitOfWork.Repository<UserRole>();
        foreach (var userRole in user.UserRoles.ToList())
        {
            userRoleRepository.Remove(userRole);
        }

        user.UserRoles.Clear();

        userRepository.Remove(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task AssignRolesAsync(User user, IEnumerable<string> roleNames)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        var userRoleRepository = _unitOfWork.Repository<UserRole>();

        var normalizedRoles = roleNames
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        var availableRoles = await roleRepository.AsQueryable()
            .Where(r => normalizedRoles.Contains(r.RoleName.ToLowerInvariant()))
            .ToListAsync();

        if (availableRoles.Count != normalizedRoles.Count)
        {
            var existing = availableRoles.Select(r => r.RoleName.ToLowerInvariant()).ToHashSet();
            var missing = normalizedRoles.Where(r => !existing.Contains(r));
            throw new KeyNotFoundException($"No se encontraron los roles: {string.Join(", ", missing)}");
        }

        foreach (var userRole in user.UserRoles.ToList())
        {
            userRoleRepository.Remove(userRole);
        }

        user.UserRoles.Clear();

        foreach (var role in availableRoles)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                AssignedAt = DateTime.UtcNow
            });
        }
    }

    private static UserDto MapToDto(User user) => new()
    {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        CreatedAt = user.CreatedAt,
        Roles = user.UserRoles.Select(ur => ur.Role.RoleName)
    };
}
