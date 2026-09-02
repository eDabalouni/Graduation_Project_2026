using KanbanApp.Application.DTOs.Auth;
using KanbanApp.Application.Interfaces;
using KanbanApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace KanbanApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            return BadRequest(new { message = " The email is already exists" });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { message = "Failed create account  ", errors });
        }

        var token = _tokenService.CreateToken(user);
        var expiresInMinutes = double.Parse(
            HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:ExpiresInMinutes"]!);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes),
            Email = user.Email,
            FullName = user.FullName
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return Unauthorized(new { message = "The email or the password don't true" });
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
            return Unauthorized(new { message = "The email or the password don't true" });
        }

        var token = _tokenService.CreateToken(user);
        var expiresInMinutes = double.Parse(
            HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:ExpiresInMinutes"]!);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes),
            Email = user.Email!,
            FullName = user.FullName
        });
    }

    [Authorize]
[HttpGet("me")]
public ActionResult<object> GetCurrentUser()
{
    var userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
    var email = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
    var fullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

    return Ok(new { userId, email, fullName });
}
}
