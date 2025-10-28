namespace Lab10_RodrigoApaza.Application.DTOs;

public class TicketUpdateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public DateTime? ClosedAt { get; set; }
}