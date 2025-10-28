using Lab10_RodrigoApaza.Application.DTOs;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lab10_RodrigoApaza.Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;

    public TicketService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TicketDto>> GetAllAsync()
    {
        var tickets = await BuildTicketQuery()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    public async Task<TicketDto?> GetByIdAsync(Guid id)
    {
        var ticket = await BuildTicketQuery()
            .FirstOrDefaultAsync(t => t.TicketId == id);

        return ticket is null ? null : MapToDto(ticket);
    }

    public async Task<IEnumerable<TicketDto>> GetByUserAsync(Guid userId)
    {
        var tickets = await BuildTicketQuery()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    public async Task<TicketDto> CreateAsync(Guid userId, TicketCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("El título del ticket es obligatorio.", nameof(dto.Title));
        }

        var ticket = new Ticket
        {
            TicketId = Guid.NewGuid(),
            UserId = userId,
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Status = "open",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Ticket>().AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return (await GetByIdAsync(ticket.TicketId))!;
    }

    public async Task<TicketDto?> UpdateAsync(Guid id, TicketUpdateDto dto)
    {
        var ticketRepository = _unitOfWork.Repository<Ticket>();
        var ticket = await ticketRepository.AsQueryable()
            .FirstOrDefaultAsync(t => t.TicketId == id);

        if (ticket is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            ticket.Title = dto.Title.Trim();
        }

        if (dto.Description is not null)
        {
            ticket.Description = dto.Description;
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            var normalizedStatus = dto.Status.Trim();
            ticket.Status = normalizedStatus;

            if (string.Equals(normalizedStatus, "closed", StringComparison.OrdinalIgnoreCase) && ticket.ClosedAt is null)
            {
                ticket.ClosedAt = DateTime.UtcNow;
            }
            else if (!string.Equals(normalizedStatus, "closed", StringComparison.OrdinalIgnoreCase))
            {
                ticket.ClosedAt = null;
            }
        }

        if (dto.ClosedAt.HasValue)
        {
            ticket.ClosedAt = dto.ClosedAt;
        }

        ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var ticketRepository = _unitOfWork.Repository<Ticket>();
        var responseRepository = _unitOfWork.Repository<Response>();

        var ticket = await ticketRepository.AsQueryable()
            .Include(t => t.Responses)
            .FirstOrDefaultAsync(t => t.TicketId == id);

        if (ticket is null)
        {
            return false;
        }

        foreach (var response in ticket.Responses.ToList())
        {
            responseRepository.Remove(response);
        }

        ticketRepository.Remove(ticket);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private IQueryable<Ticket> BuildTicketQuery()
    {
        return _unitOfWork.Repository<Ticket>()
            .AsQueryable()
            .Include(t => t.User)
            .Include(t => t.Responses)
            .ThenInclude(r => r.Responder);
    }

    private static TicketDto MapToDto(Ticket ticket) => new()
    {
        TicketId = ticket.TicketId,
        UserId = ticket.UserId,
        Title = ticket.Title,
        Description = ticket.Description,
        Status = ticket.Status,
        CreatedAt = ticket.CreatedAt,
        ClosedAt = ticket.ClosedAt,
        Username = ticket.User?.Username,
        Responses = ticket.Responses
            .OrderBy(r => r.CreatedAt)
            .Select(r => new ResponseDto
            {
                ResponseId = r.ResponseId,
                TicketId = r.TicketId,
                ResponderId = r.ResponderId,
                Message = r.Message,
                CreatedAt = r.CreatedAt,
                ResponderUsername = r.Responder?.Username
            })
    };
}