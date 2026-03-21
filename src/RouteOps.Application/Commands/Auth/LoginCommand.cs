// src/RouteOps.Application/Commands/Auth/LoginCommand.cs
using MediatR;

namespace RouteOps.Application.Commands.Auth;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public record LoginResult(
    string Id,
    string Name,
    string Email,
    string Role,
    string Token
);
