using AutoPartsSystem.Data;
using AutoPartsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRepository _repo;

    public AuthController(IRepository repo)
    {
        _repo = repo;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _repo.GetUserByLogin(request.Login);
        if (user == null || user.PasswordHash != request.Password)
            return Unauthorized(new { message = "Неверный логин или пароль" });

        // Store user info in session
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserLogin", user.Login);
        HttpContext.Session.SetString("UserRole", user.Role);

        return Ok(new { id = user.Id, login = user.Login, role = user.Role });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
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
