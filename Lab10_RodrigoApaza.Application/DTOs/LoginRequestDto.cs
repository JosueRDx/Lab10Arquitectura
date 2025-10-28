using System.ComponentModel.DataAnnotations;

namespace Lab10_RodrigoApaza.Application.DTOs;

public class LoginRequestDto
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}