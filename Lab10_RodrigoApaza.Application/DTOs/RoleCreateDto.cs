using System.ComponentModel.DataAnnotations;

namespace Lab10_RodrigoApaza.Application.DTOs;

public class RoleCreateDto
{
    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;
}