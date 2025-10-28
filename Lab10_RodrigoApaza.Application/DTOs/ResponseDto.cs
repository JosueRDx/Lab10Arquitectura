namespace Lab10_RodrigoApaza.Application.DTOs;

public class ResponseDto
{
    public Guid ResponseId { get; init; }
    public Guid TicketId { get; init; }
    public Guid ResponderId { get; init; }
    public required string Message { get; init; }
    public DateTime? CreatedAt { get; init; }
    public string? ResponderUsername { get; init; }
}