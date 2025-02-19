namespace TestIdentityCore.Api.Dto;

public class UserReadDto
{
    public required string Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public ICollection<string> Roles { get; set; } = [];
}