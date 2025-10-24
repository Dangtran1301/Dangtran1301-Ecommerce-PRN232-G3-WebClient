using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController(IUserService userService, ILogger<UserController> logger) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(UserFilterDto filter, CancellationToken cancellationToken)
        {
            filter.PageIndex = filter.PageIndex == 0 ? 1 : filter.PageIndex;
            filter.PageSize = filter.PageSize == 0 ? 10 : filter.PageSize;

            var result = await userService.GetUsersAsync(filter, cancellationToken);
            if (result.Success) return View(result.Data);
            ViewBag.Error = result.Error?.Message;
            return View(new PagedResult<UserDto>([], 0, 0, 0));
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(dto);
            dto = dto with { FullName = dto.FullName?.Trim() ?? "" };
            var result = await userService.CreateAsync(dto, cancellationToken);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error creating user");
                return View(dto);
            }

            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            var user = await userService.GetByIdAsync(id, cancellationToken);
            if (!user.Success || user.Data == null) return NotFound();

            var dto = new UpdateUserRequest(user.Data.FullName, user.Data.PhoneNumber, user.Data.Avatar);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UpdateUserRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await userService.UpdateAsync(id, dto, cancellationToken);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error updating user");
                return View(dto);
            }

            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await userService.DeleteAsync(id, cancellationToken);
            if (!result.Success)
                TempData["ErrorMessage"] = result.Error?.Message ?? "Error deleting user";
            else
                TempData["SuccessMessage"] = "User deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}
