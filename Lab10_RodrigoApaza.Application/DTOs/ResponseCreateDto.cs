using System.ComponentModel.DataAnnotations;

namespace Lab10_RodrigoApaza.Application.DTOs;

public class ResponseCreateDto
{
    [Required]
    public Guid TicketId { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;
}