// src/RouteOps.Domain/Entities/User.cs
namespace RouteOps.Domain.Entities;

public class User
{
    public Guid   Id           { get; private set; } = Guid.NewGuid();
    public string Name         { get; private set; } = default!;
    public string Email        { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Role         { get; private set; } = "operator";
    public bool   Active       { get; private set; } = true;
    public DateTime CreatedAt  { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt  { get; private set; } = DateTime.UtcNow;

    private User() { }

    public static User Create(string name, string email,
        string passwordHash, string role = "operator") =>
        new()
        {
            Name         = name.Trim(),
            Email        = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role         = role,
        };
}
