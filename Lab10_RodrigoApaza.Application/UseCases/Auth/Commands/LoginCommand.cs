using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Auth.Commands;

// Este comando necesita el DTO con los datos de inicio de sesión
public record LoginCommand(LoginRequestDto LoginRequest) : IRequest<LoginResponseDto?>;