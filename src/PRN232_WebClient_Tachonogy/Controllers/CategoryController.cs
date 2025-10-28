using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers
{
    public class CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger) : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var filter = new CategoryFilterDto
            {
                Keyword = keyword,
                PageIndex = page,
                PageSize = pageSize,
                OrderBy = "CategoryName",
                Descending = false
            };

            var result = await categoryService.GetCategoriesAsync(filter, cancellationToken);
            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.MaxPage = result.Data?.TotalPages ?? 1;

            if (result.Success)
                return View(result.Data);

            ViewBag.Error = result.Error?.Message;
            return View(new PagedResult<CategoryDto>([], 0, 0, 0));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() => View();
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(dto);

            dto = dto with
            {
                CategoryName = dto.CategoryName?.Trim() ?? "",
                CategoryDescription = dto.CategoryDescription?.Trim(),
            };

            var result = await categoryService.CreateAsync(dto, cancellationToken);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error creating category");
                return View(dto);
            }

            TempData["SuccessMessage"] = "Category created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            var category = await categoryService.GetByIdAsync(id, cancellationToken);
            if (!category.Success || category.Data == null)
                return NotFound();

            var dto = new UpdateCategoryRequest
            {
                CategoryName = category.Data.CategoryName,
                CategoryDescription = category.Data.CategoryDescription
            };
            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, UpdateCategoryRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await categoryService.UpdateAsync(id, dto, cancellationToken);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error updating category");
                return View(dto);
            }

            TempData["SuccessMessage"] = "Category updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await categoryService.DeleteAsync(id, cancellationToken);
            if (!result.Success)
                TempData["ErrorMessage"] = result.Error?.Message ?? "Error deleting category";
            else
                TempData["SuccessMessage"] = "Category deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}
