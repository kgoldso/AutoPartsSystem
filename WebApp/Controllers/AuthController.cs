using AutoPartsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IdentityService identityService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await identityService.LoginAsync(request.Login, request.Password);
        
        if (!result.IsSuccess)
            return Unauthorized(new { message = result.Error });

        var user = result.Value!;

        // Store user info in session
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserLogin", user.Login);
        HttpContext.Session.SetString("UserRole", user.Role);

        return Ok(new { id = user.Id, login = user.Login, role = user.Role });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        identityService.Logout();
        HttpContext.Session.Clear();
        return Ok(new { message = "Выход выполнен" });
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized(new { message = "Не авторизован" });

        var login = HttpContext.Session.GetString("UserLogin");
        var role = HttpContext.Session.GetString("UserRole");

        return Ok(new { id = userId, login, role });
    }
}

public class LoginRequest
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
}
