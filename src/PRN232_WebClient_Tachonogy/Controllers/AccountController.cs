using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers;

public class AccountController(IAuthService authService, IUserService userService, IHttpContextAccessor httpContextAccessor)
    : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        var accessToken = Request.Cookies["AccessToken"];
        if (!string.IsNullOrEmpty(accessToken))
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var response = await authService.LoginAsync(dto, cancellationToken);
        if (!response.Success)
        {
            ModelState.AddModelError("", response.Error?.Message ?? "Login failed.");
            return View(dto);
        }
        var user = response.Data.User;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("FullName", user.FullName),
            new("AccessToken", response.Data.AccessToken),
            new("RefreshToken", response.Data.RefreshToken)
        };

        var identity = new ClaimsIdentity(claims, "CookieAuth");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("CookieAuth", principal);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["RefreshToken"];

        if (!string.IsNullOrEmpty(refreshToken))
            await authService.LogoutAsync(new RefreshTokenRequestDto { RefreshToken = refreshToken });

        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(CreateUserRequest dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await authService.RegisterAsync(dto, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Registration failed");
            return View(dto);
        }

        TempData["SuccessMessage"] = "Registration successfully! Please login.";
        return RedirectToAction("Login");
    }
}