using Lab10_RodrigoApaza.Application.DTOs;
using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Responses.Queries;

// Necesita el ResponseId para buscar la respuesta específica
public record GetResponseByIdQuery(Guid ResponseId) : IRequest<ResponseDto?>;