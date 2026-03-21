// src/RouteOps.Application/Commands/Auth/LoginHandler.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RouteOps.Application.Interfaces;

namespace RouteOps.Application.Commands.Auth;

public sealed class LoginHandler(
    IRouteOpsDbContext db,
    IJwtService jwtService)
    : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.Active, ct)
            ?? throw new UnauthorizedAccessException("Email o contraseña incorrectos.");

        // Verificar password con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(cmd.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email o contraseña incorrectos.");

        var token = jwtService.GenerateToken(
                    user.Id.ToString(), user.Email, user.Name, user.Role);

        return new LoginResult(
            Id: user.Id.ToString(),
            Name: user.Name,
            Email: user.Email,
            Role: user.Role,
            Token: token
        );
    }
}
