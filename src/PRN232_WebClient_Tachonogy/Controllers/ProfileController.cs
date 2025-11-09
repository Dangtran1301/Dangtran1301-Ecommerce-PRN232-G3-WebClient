using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers
{
    public class ProfileController(IUserProfileService profileService, ITokenProvider tokenProvider) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var userId = tokenProvider.GetUserId();
            if (userId == Guid.Empty)
            {
                ViewBag.ErrorMessage = "You must be logged in to view profile.";
                return RedirectToAction("Login", "Auth");
            }

            var response = await profileService.GetByIdAsync(userId, cancellationToken);
            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = "Failed to load profile.";
                return RedirectToAction("Index", "Home");
            }

            return View(response.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(CancellationToken cancellationToken)
        {
            var userId = tokenProvider.GetUserId();
            var response = await profileService.GetByIdAsync(userId, cancellationToken);
            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = "Failed to load profile.";
                return RedirectToAction("Index");
            }

            var model = new UpdateUserProfileRequest(
                 response.Data.FullName,
                 response.Data.PhoneNumber,
                 response.Data.Avatar,
                 response.Data.Gender,
                 response.Data.DayOfBirth,
                 response.Data.Address
            );
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateUserProfileRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(request);

            var userId = tokenProvider.GetUserId();
            var result = await profileService.UpdateAsync(userId, request, cancellationToken);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Error?.Message ?? "Failed to update profile.";
                return View(request);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }
    }
}