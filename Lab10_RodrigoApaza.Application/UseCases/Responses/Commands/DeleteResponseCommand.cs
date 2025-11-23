using MediatR;

namespace Lab10_RodrigoApaza.Application.UseCases.Responses.Commands;

// Necesita el ResponseId para eliminar la respuesta específica
public record DeleteResponseCommand(Guid ResponseId) : IRequest<bool>;