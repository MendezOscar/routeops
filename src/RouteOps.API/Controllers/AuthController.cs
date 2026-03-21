// src/RouteOps.API/Controllers/AuthController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RouteOps.Application.Commands.Auth;

namespace RouteOps.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest req, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new LoginCommand(req.Email, req.Password), ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    // Temporal — eliminar después
    [HttpGet("hash/{password}")]
    public IActionResult Hash(string password) =>
        Ok(new { hash = BCrypt.Net.BCrypt.HashPassword(password, 12) });
}

public record LoginRequest(string Email, string Password);
