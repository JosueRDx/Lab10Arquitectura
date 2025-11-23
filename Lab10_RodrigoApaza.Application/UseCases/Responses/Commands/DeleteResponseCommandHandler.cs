using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Application.UseCases.Responses.Commands;

public class DeleteResponseCommandHandler : IRequestHandler<DeleteResponseCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteResponseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lógica para manejar el comando DeleteResponseCommand
    public async Task<bool> Handle(DeleteResponseCommand request, CancellationToken cancellationToken)
    {
        var responseRepository = _unitOfWork.Repository<Response>();
        
        // 1. Encontrar la respuesta
        var response = await responseRepository.AsQueryable()
            .FirstOrDefaultAsync(r => r.ResponseId == request.ResponseId, cancellationToken);

        if (response is null)
        {
            return false;
        }

        // 2. Eliminar la respuesta
        responseRepository.Remove(response);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}