namespace Lab10_RodrigoApaza.Application.DTOs;

public class UserDto
{
    public Guid UserId { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
    public DateTime? CreatedAt { get; init; }
    public IEnumerable<string> Roles { get; init; } = Enumerable.Empty<string>();
}