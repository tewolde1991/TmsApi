using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.AuthDtos;
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identites;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
// [Route("api/{version:apiVersion}/auth")]
[Route("api/auth")]
public class AuthController : ControllerBase
{    private readonly UserManager<TmsUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TmsDbContext _context;
    private readonly TokenService _tokenService;
private const string RefreshTokenCookieName = "tms_refresh";

private void SetRefreshTokenCookie(string token, DateTime expiresAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,  //js can't read it
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt,
            Path = "/api/auth/refresh"
        };
        Response.Cookies.Append(RefreshTokenCookieName, token, cookieOptions);
    }
    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName);
    }
    public AuthController(
UserManager<TmsUser> userManager,
RoleManager<IdentityRole> roleManager,
TmsDbContext context,
TokenService tokenService

    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _tokenService = tokenService;

    }
    public record RegisterRequest(
string Email,
string Password,
string FirstName,
string LastName,
string Role
    );

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            // prevent account enumeration by returning a generic response
            return Ok(new { message = "Registration request received." });
        }
        var user = new TmsUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        // ensure requested role exists
        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(request.Role));
        }
        await _userManager.AddToRoleAsync(user, request.Role);
        return Ok(new { message = "Registration sucessful." });
    }




    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(new { detail = "Invalid credentials." });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(423, new { detail = "Account locked due to multiple failed login attempts. Try again in 15 minutes." });
        }
        var validPassword = await _userManager.CheckPasswordAsync(user, request.password);

        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);
            return Unauthorized(new { detail = "Invalid credentials." });
        }
        // reset failed attempt counter on sucessful login
        await _userManager.ResetAccessFailedCountAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateJwt(user, roles);

        // issue intial refresh token
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
        SetRefreshTokenCookie(refreshToken.Token, refreshToken.ExpiresAt);

        return Ok(new
        {
            accessToken
            // refreshToken = refreshToken.Token
        });
    }
    public record RefreshRequest(string RefreshToken);
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
    // Read the refresh token from the HttpOnly cookie
    var refreshTokenValue = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrEmpty(refreshTokenValue))
        {
            return Unauthorized (new {CourseDetailDto = "No refresh token provided."});
        }
        var storedToken = await _context.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue);
        

        if (storedToken == null)
        {
            return Unauthorized(new { detail = "Invalid refresh token." });

        }

        // theft detection if an alredy used token issubmitted, revoke all tokens for this user
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
            return Unauthorized(new { detail = "Token theft detected. All user sessions revoked." });
        }
        if (storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized(new { detail = "Refreshtoken expired or revoked." });
        }
        // mark current token as used
        storedToken.IsUsed = true;

        // issue brand new refresh token pair
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
        // issue httponly cookie with the new refresh toke
        SetRefreshTokenCookie(newRefreshToken.Token, newRefreshToken.ExpiresAt);
        var user = await _userManager.FindByIdAsync(storedToken.UserId);

        var roles = await _userManager.GetRolesAsync(user!);
        var newAccessToken = _tokenService.GenerateJwt(user!, roles);
        return Ok(new
        {
            accessToken = newAccessToken,
            // refreshToken = newRefreshToken.Token
        });
    }
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    var refreshTokenValue = Request.Cookies[RefreshTokenCookieName];
    if (!string.IsNullOrEmpty(refreshTokenValue))
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue);
        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }

    ClearRefreshTokenCookie();
    return Ok(new { message = "Logged out successfully." });
}
}