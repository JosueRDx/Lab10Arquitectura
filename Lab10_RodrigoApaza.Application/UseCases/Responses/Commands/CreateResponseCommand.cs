using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Responses.Commands;

// Necesita el ResponderId y el DTO con los datos de la nueva respuesta
public record CreateResponseCommand(Guid ResponderId, ResponseCreateDto Dto) : IRequest<ResponseDto>;