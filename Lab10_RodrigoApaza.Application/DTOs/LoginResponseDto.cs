namespace Lab10_RodrigoApaza.Application.DTOs;

public class LoginResponseDto
{
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required string Username { get; init; }
    public IEnumerable<string> Roles { get; init; } = Enumerable.Empty<string>();
}