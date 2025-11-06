using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers;

public class StockController(IStockService service, ILogger<StockController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(Guid? productId, string? location, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var filter = new StockFilterDto
        {
            ProductId = productId,
            Location = location,
            PageIndex = page,
            PageSize = pageSize,
            OrderBy = "CreatedAt",
            Descending = false
        };

        var result = await service.GetStocksAsync(filter, cancellationToken);
        ViewBag.ProductId = productId;
        ViewBag.Location = location;
        ViewBag.Page = page;
        ViewBag.MaxPage = result.Data?.TotalPages ?? 1;

        if (result.Success)
            return View(result.Data);

        ViewBag.Error = result.Error?.Message;
        return View(new PagedResult<StockDto>([], 0, 0, 0));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateStockRequest dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        dto = dto with
        {
            Location = dto.Location?.Trim()
        };

        var result = await service.CreateAsync(dto, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Error creating stock");
            return View(dto);
        }

        TempData["SuccessMessage"] = "Stock created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var stock = await service.GetByIdAsync(id, cancellationToken);
        if (!stock.Success || stock.Data == null)
            return NotFound();

        var dto = new UpdateStockRequest
        {
            Quantity = stock.Data.Quantity,
            Location = stock.Data.Location?.Trim()
        };
        return View(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, UpdateStockRequest dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await service.UpdateAsync(id, dto, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Error updating stock");
            return View(dto);
        }

        TempData["SuccessMessage"] = "Stock updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            TempData["ErrorMessage"] = result.Error?.Message ?? "Error deleting stock";
        else
            TempData["SuccessMessage"] = "Stock deleted successfully!";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null)
        {
            TempData["ErrorMessage"] = "Stock not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }
}


