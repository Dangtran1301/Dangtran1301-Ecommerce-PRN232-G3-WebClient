using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Services.Interfaces;
using System.Security.Claims;

namespace PRN232_WebClient_Tachonogy.Controllers;

public class AccountController(IAuthService authService, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor)
    : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        var accessToken = tokenProvider.AccessToken;
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
        if (!response.Success || response.Data is null)
        {
            ViewBag.ErrorMessages = response.Error?.Message ?? "Login failed.";
            return View(dto);
        }

        var user = response.Data.User;

        tokenProvider.SetTokens(response.Data.AccessToken, response.Data.RefreshToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("FullName", user.FullName),
        };

        var identity = new ClaimsIdentity(claims, "CookieAuth");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("CookieAuth", principal);

        TempData["SuccessMessages"] = $"Welcome back, {user.FullName ?? user.UserName}!";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = tokenProvider.RefreshToken;

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await authService.LogoutAsync(new RefreshTokenRequestDto { RefreshToken = refreshToken }, cancellationToken);
        }

        tokenProvider.ClearTokens();
        await HttpContext.SignOutAsync("CookieAuth");

        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await authService.RegisterAsync(dto, cancellationToken);
        if (!result.Success)
        {
            ViewBag.ErrorMessages = result.Error?.Message ?? "Registration failed.";
            return View(dto);
        }

        TempData["SuccessMessages"] = "Registration successful! Please login.";
        return RedirectToAction("Login");
    }
}