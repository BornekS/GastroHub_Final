using Microsoft.AspNetCore.Mvc;          // ApiController, ControllerBase, ActionResult<T>, Ok()
using Microsoft.AspNetCore.Authorization; // [Authorize]
using System.Security.Claims;            // ClaimTypes
using GastroHub.Services.Interfaces;     // IAuthService
using GastroHub.Dtos.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        var user = await _auth.RegisterAsync(dto);
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto dto)
    {
        var user = await _auth.LoginAsync(dto);
        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);

        var user = await _auth.GetUserAsync(email);

        return Ok(user);
    }
}