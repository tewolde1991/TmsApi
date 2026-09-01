using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.AuthDtos;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identites;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
private readonly UserManager<TmsUser> _userManager;
private readonly RoleManager<IdentityRole> _roleManager;
private readonly TmsDbContext _context;
private readonly TokenService _tokenService;
public AuthController(
UserManager<TmsUser> userManager,
RoleManager<IdentityRole> roleManager,
TmsDbContext context,
TokenService tokenService)
{
_userManager = userManager;
_roleManager = roleManager;
_context = context;
_tokenService = tokenService;
}
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest
request)
{
var user = await _userManager.FindByEmailAsync(request.Email);if (user == null) return Unauthorized(new { detail = "Invalidcredentials." });
if (await _userManager.IsLockedOutAsync(user))
{
return StatusCode(423, new { detail = "Account locked dueto multiple failed login attempts." });
}
var validPassword = await _userManager.CheckPasswordAsync(user,request.Password);
if (!validPassword)
{
await _userManager.AccessFailedAsync(user);
return Unauthorized(new { detail = "Invalidcredentials." });
}
await _userManager.ResetAccessFailedCountAsync(user);
var roles = await _userManager.GetRolesAsync(user);
var accessToken = _tokenService.GenerateJwt(user, roles);
// Issue initial Refresh Token
var refreshToken = new RefreshToken
{
Token = Guid.NewGuid().ToString("N"),
UserId = user.Id,
ExpiresAt = DateTime.UtcNow.AddDays(7),
IsUsed = false,
IsRevoked = false
};
_context.RefreshTokens.Add(refreshToken);
await _context.SaveChangesAsync();
return Ok(new
{
accessToken,
refreshToken = refreshToken.Token
});
}
public record RefreshRequest(string RefreshToken);
[HttpPost("refresh")]
public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
{
var storedToken = await _context.RefreshTokens
.FirstOrDefaultAsync(rt => rt.Token ==
request.RefreshToken);
if (storedToken == null)
{
return Unauthorized(new { detail = "Invalid refreshtoken." });
}
// Theft Detection: If an ALREADY-USED token is submitted,revoke ALL tokens for this user!
if (storedToken.IsUsed)
{
var userTokens = await _context.RefreshTokens
.Where(rt => rt.UserId == storedToken.UserId)
.ToListAsync();
foreach (var t in userTokens)
{
t.IsRevoked = true;
}
await _context.SaveChangesAsync();
return Unauthorized(new { detail = "Token theft detected.All user sessions revoked." });
}
if (storedToken.IsRevoked || storedToken.ExpiresAt <
DateTime.UtcNow)
{
return Unauthorized(new { detail = "Refresh token expiredor revoked." });
}
// Mark current token as used
storedToken.IsUsed = true;
// Issue brand-new Refresh Token pair
var newRefreshToken = new RefreshToken
{
Token = Guid.NewGuid().ToString("N"),
UserId = storedToken.UserId,
ExpiresAt = DateTime.UtcNow.AddDays(7),
IsUsed = false,
IsRevoked = false
};
_context.RefreshTokens.Add(newRefreshToken);
await _context.SaveChangesAsync();
var user = await _userManager.FindByIdAsync(storedToken.UserId);
var roles = await _userManager.GetRolesAsync(user!);
var newAccessToken = _tokenService.GenerateJwt(user!, roles);return Ok(new
{
accessToken = newAccessToken,
refreshToken = newRefreshToken.Token
});
}
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    // Check if email already taken
    var existing = await _userManager.FindByEmailAsync(request.Email);
    if (existing != null)
        return Conflict(new { detail = "Email is already registered." });

    // Validate role
    var allowedRoles = new[] { "Student", "Instructor", "Admin" };
    if (!allowedRoles.Contains(request.Role))
        return BadRequest(new { detail = $"Invalid role. Allowed: {string.Join(", ", allowedRoles)}" });

    var user = new TmsUser
    {
        UserName = request.Email,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName,
        EmailConfirmed = true
    };

    var result = await _userManager.CreateAsync(user, request.Password);
    if (!result.Succeeded)
        return BadRequest(new { detail = result.Errors.Select(e => e.Description) });

    // Ensure role exists then assign
    if (!await _roleManager.RoleExistsAsync(request.Role))
        await _roleManager.CreateAsync(new IdentityRole(request.Role));

    await _userManager.AddToRoleAsync(user, request.Role);

    return Created($"/api/auth/{user.Id}", new { message = "Registration successful." });
}

}