using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers;

public class ProductAttributeController(IProductAttributeService service, ILogger<ProductAttributeController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? keyword, Guid? productId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var filter = new ProductAttributeFilterDto
        {
            Keyword = keyword,
            ProductId = productId,
            PageIndex = page,
            PageSize = pageSize,
            OrderBy = "CreatedAt",
            Descending = false
        };

        var result = await service.GetProductAttributesAsync(filter, cancellationToken);
        ViewBag.Keyword = keyword;
        ViewBag.ProductId = productId;
        ViewBag.Page = page;
        ViewBag.MaxPage = result.Data?.TotalPages ?? 1;

        if (result.Success)
            return View(result.Data);

        ViewBag.Error = result.Error?.Message;
        return View(new PagedResult<ProductAttributeDto>([], 0, 0, 0));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductAttributeRequest dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        dto = dto with
        {
            AttributeName = dto.AttributeName?.Trim() ?? "",
            AttributeValue = dto.AttributeValue?.Trim() ?? ""
        };

        var result = await service.CreateAsync(dto, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Error creating product attribute");
            return View(dto);
        }

        TempData["SuccessMessage"] = "Product attribute created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var productAttribute = await service.GetByIdAsync(id, cancellationToken);
        if (!productAttribute.Success || productAttribute.Data == null)
            return NotFound();

        var dto = new UpdateProductAttributeRequest
        {
            AttributeName = productAttribute.Data.AttributeName.Trim(),
            AttributeValue = productAttribute.Data.AttributeValue.Trim()
        };
        return View(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, UpdateProductAttributeRequest dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await service.UpdateAsync(id, dto, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Error updating product attribute");
            return View(dto);
        }

        TempData["SuccessMessage"] = "Product attribute updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            TempData["ErrorMessage"] = result.Error?.Message ?? "Error deleting product attribute";
        else
            TempData["SuccessMessage"] = "Product attribute deleted successfully!";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null)
        {
            TempData["ErrorMessage"] = "Product attribute not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }
}


