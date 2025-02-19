namespace TestIdentityCore.Api.Dto;

public class UserCreateDto
{
    public string? FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public List<string> Roles { get; set; } = [];
}