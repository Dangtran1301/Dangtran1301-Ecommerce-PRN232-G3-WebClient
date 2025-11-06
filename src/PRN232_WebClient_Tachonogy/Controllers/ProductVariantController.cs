using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers;

public class ProductVariantController(IProductVariantService service, ILogger<ProductVariantController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? keyword, Guid? productId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var filter = new ProductVariantFilterDto
        {
            Keyword = keyword,
            ProductId = productId,
            PageIndex = page,
            PageSize = pageSize,
            OrderBy = "CreatedAt",
            Descending = false
        };

        var result = await service.GetProductVariantsAsync(filter, cancellationToken);
        ViewBag.Keyword = keyword;
        ViewBag.ProductId = productId;
        ViewBag.Page = page;
        ViewBag.MaxPage = result.Data?.TotalPages ?? 1;

        if (result.Success)
            return View(result.Data);

        ViewBag.Error = result.Error?.Message;
        return View(new PagedResult<ProductVariantDto>([], 0, 0, 0));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductVariantRequest dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        dto = dto with
        {
            VariantName = dto.VariantName?.Trim() ?? "",
            Sku = dto.Sku?.Trim(),
            ImageUrl = dto.ImageUrl?.Trim()
        };

        var result = await service.CreateAsync(dto, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Error creating product variant");
            return View(dto);
        }

        TempData["SuccessMessage"] = "Product variant created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var productVariant = await service.GetByIdAsync(id, cancellationToken);
        if (!productVariant.Success || productVariant.Data == null)
            return NotFound();

        var dto = new UpdateProductVariantRequest
        {
            VariantName = productVariant.Data.VariantName.Trim(),
            Price = productVariant.Data.Price,
            Sku = productVariant.Data.Sku?.Trim(),
            ImageUrl = productVariant.Data.ImageUrl?.Trim()
        };
        return View(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, UpdateProductVariantRequest dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await service.UpdateAsync(id, dto, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Error updating product variant");
            return View(dto);
        }

        TempData["SuccessMessage"] = "Product variant updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            TempData["ErrorMessage"] = result.Error?.Message ?? "Error deleting product variant";
        else
            TempData["SuccessMessage"] = "Product variant deleted successfully!";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null)
        {
            TempData["ErrorMessage"] = "Product variant not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }
}

