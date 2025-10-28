using System.ComponentModel.DataAnnotations;

namespace Lab10_RodrigoApaza.Application.DTOs;

public class UserUpdateDto
{
    [EmailAddress]
    public string? Email { get; set; }

    public string? Password { get; set; }

    public IEnumerable<string>? Roles { get; set; }
}