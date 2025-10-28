using Lab10_RodrigoApaza.Application.DTOs;

namespace Lab10_RodrigoApaza.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest);
}