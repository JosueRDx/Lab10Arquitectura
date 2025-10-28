namespace Lab10_RodrigoApaza.Application.DTOs;

public class TicketDto
{
    public Guid TicketId { get; init; }
    public Guid UserId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required string Status { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public string? Username { get; init; }
    public IEnumerable<ResponseDto> Responses { get; init; } = Enumerable.Empty<ResponseDto>();
}
