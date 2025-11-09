using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController(IAdminService adminService, ILogger<UsersController> logger) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var result = await adminService.GetAllAsync(cancellationToken);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Error?.Message ?? "Failed to load user list.";
                return View(new List<AuthUserResponse>());
            }

            return View(result.Data);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var result = await adminService.GetByIdAsync(id, cancellationToken);
            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.Error?.Message ?? "User not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        [HttpGet("create")]
        public IActionResult Create() => View();

        [HttpPost("create")]
        public async Task<IActionResult> Create(RegisterRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(request);

            var result = await adminService.CreateAsync(request, cancellationToken);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Failed to create user.");
            return View(request);
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            var result = await adminService.GetByIdAsync(id, cancellationToken);
            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.Error?.Message ?? "User not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        [HttpPost("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id, AuthUserResponse model, CancellationToken cancellationToken)
        {
            var roleRequest = new RemoteAuthUserRequest(model.Role);
            var roleResult = await adminService.UpdateRoleAsync(id, roleRequest, cancellationToken);

            var statusRequest = new UpdateStatusAuthUserRequest(model.AccountStatus);
            var statusResult = await adminService.UpdateStatusAsync(id, statusRequest, cancellationToken);

            if (!roleResult.Success || !statusResult.Success)
            {
                ViewBag.ErrorMessage =
                    $"{roleResult.Error?.Message ?? ""} {statusResult.Error?.Message ?? ""}".Trim();
                return View(model);
            }

            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{id:guid}/delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await adminService.DeleteAsync(id, cancellationToken);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Error?.Message ?? "Failed to delete user.";
            }
            else
            {
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter(UserFilterRequest filter, CancellationToken cancellationToken)
        {
            var result = await adminService.FilterAsync(filter, cancellationToken);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Error?.Message ?? "Failed to filter users.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Keyword = filter.UserName;
            return View("Index", result.Data);
        }
    }
}
