using System.ComponentModel.DataAnnotations;

namespace Lab10_RodrigoApaza.Application.DTOs;

public class TicketCreateDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}