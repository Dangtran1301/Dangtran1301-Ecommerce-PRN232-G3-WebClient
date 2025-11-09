using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Services.Interfaces;
using System.Security.Claims;

namespace PRN232_WebClient_Tachonogy.Controllers;

public class AuthController(IAuthService authService, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor)
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

        tokenProvider.SetTokens(response.Data.AccessToken, response.Data.RefreshToken);
        await HttpContext.SignInAsync("CookieAuth", principal);

        TempData["SuccessMessages"] = $"Welcome back, {user.FullName}";
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
            tokenProvider.ClearTokens();
        }

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

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = tokenProvider.GetUserId();
        if (userId.Equals(Guid.Empty))
        {
            ModelState.AddModelError("", "Cannot determine current user.");
            return View(model);
        }

        var result = await authService.ChangePasswordAsync(userId, model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Index", "Profile");
        }

        ModelState.AddModelError("", result.Error?.Message ?? "An error occurred while changing password.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var clientUri = $"{Request.Scheme}://{Request.Host}";
        var request = model with { ClientUri = clientUri };

        var response = await authService.ForgotPasswordAsync(request, cancellationToken);

        if (response.Success)
        {
            TempData["SuccessMessages"] = "If an account exists with that email, a reset link has been sent.";
            return RedirectToAction("Login");
        }

        ViewBag.ErrorMessages = response.Error?.Message ?? "Failed to send reset password email.";
        return View(model);
    }

    [HttpGet("reset-password")]
    [AllowAnonymous]
    public IActionResult ResetPassword([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            TempData["ErrorMessages"] = "Invalid or missing token.";
            return RedirectToAction("Login");
        }

        var model = new ResetPasswordRequest(token, string.Empty);
        return View(model);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var response = await authService.ResetPasswordAsync(model, cancellationToken);

        if (response.Success)
        {
            TempData["SuccessMessages"] = "Your password has been reset successfully! Please login.";
            return RedirectToAction("Login");
        }

        ViewBag.ErrorMessages = response.Error?.Message ?? "Failed to reset password.";
        return View(model);
    }
}