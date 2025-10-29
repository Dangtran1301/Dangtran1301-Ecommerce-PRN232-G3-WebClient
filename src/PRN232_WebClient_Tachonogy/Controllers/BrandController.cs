using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers
{
    public class BrandController(IBrandService brandService, ILogger<BrandController> logger) : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var filter = new BrandFilterDto
            {
                Keyword = keyword,
                PageIndex = page,
                PageSize = pageSize,
                OrderBy = "BrandName",
                Descending = false
            };

            var result = await brandService.GetBrandsAsync(filter, cancellationToken);
            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.MaxPage = result.Data?.TotalPages ?? 1;

            if (result.Success)
                return View(result.Data);

            ViewBag.Error = result.Error?.Message;
            return View(new PagedResult<BrandDto>([], 0, 0, 0));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() => View();
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateBrandRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(dto);

            dto = dto with
            {
                BrandName = dto.BrandName?.Trim() ?? "",
                BrandDescription = dto.BrandDescription?.Trim(),
                WebsiteUrl = dto.WebsiteUrl?.Trim(),
            };

            var result = await brandService.CreateAsync(dto, cancellationToken);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error creating Brand");
                return View(dto);
            }

            TempData["SuccessMessage"] = "Brand created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            var Brand = await brandService.GetByIdAsync(id, cancellationToken);
            if (!Brand.Success || Brand.Data == null)
                return NotFound();

            var dto = new UpdateBrandRequest
            {
                BrandName = Brand.Data.BrandName.Trim(),
                BrandDescription = Brand.Data.BrandDescription.Trim(),
                WebsiteUrl = Brand.Data.WebsiteUrl.Trim(),
            };
            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, UpdateBrandRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await brandService.UpdateAsync(id, dto, cancellationToken);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error updating Brand");
                return View(dto);
            }

            TempData["SuccessMessage"] = "Brand updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await brandService.DeleteAsync(id, cancellationToken);
            if (!result.Success)
                TempData["ErrorMessage"] = result.Error?.Message ?? "Error deleting Brand";
            else
                TempData["SuccessMessage"] = "Brand deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}
