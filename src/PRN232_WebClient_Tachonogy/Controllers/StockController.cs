using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers;

public class StockController(
    IStockService service,
    IProductService productService,
    ILogger<StockController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? location, string? keyword, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        // Load all products at once for better performance
        var productsDict = new Dictionary<Guid, ProductDto>();
        try
        {
            var allProductsResult = await productService.GetAllAsync(cancellationToken);
            if (allProductsResult.Success && allProductsResult.Data != null)
            {
                foreach (var product in allProductsResult.Data)
                {
                    productsDict[product.Id] = product;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading all products for stock index");
        }

        List<(StockDto Stock, ProductDto? Product)> stocksWithProducts;
        
        // If keyword is provided, load stocks in batches and filter client-side
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            // Load stocks in smaller batches for keyword search (limit to reasonable number)
            var allStocks = new List<StockDto>();
            var batchSize = 25; // Use smaller batch size to avoid 400 errors
            var maxBatches = 40; // Limit to 40 batches (1000 items max)
            var currentPage = 1;
            var hasMore = true;
            var batchCount = 0;
            
            while (hasMore && batchCount < maxBatches)
            {
                try
                {
                    var filter = new StockFilterDto
                    {
                        Location = !string.IsNullOrWhiteSpace(location) ? location : null, // Only include location if provided
                        Keyword = null, // Don't send keyword to API, filter client-side
                        PageIndex = currentPage,
                        PageSize = batchSize,
                        OrderBy = "Quantity",
                        Descending = false
                    };
                    
                    var batchResult = await service.GetStocksAsync(filter, cancellationToken);
                    if (batchResult.Success && batchResult.Data != null)
                    {
                        var items = batchResult.Data.Items?.ToList() ?? new List<StockDto>();
                        if (items.Any())
                        {
                            allStocks.AddRange(items);
                            // Check if there are more items to load
                            var batchTotalCount = batchResult.Data.TotalCount;
                            hasMore = items.Count == batchSize && batchTotalCount > allStocks.Count;
                            currentPage++;
                            batchCount++;
                        }
                        else
                        {
                            hasMore = false;
                        }
                    }
                    else
                    {
                        logger.LogWarning("Failed to load stock batch {BatchNumber}: {Error}", currentPage, batchResult.Error?.Message);
                        hasMore = false;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error loading stock batch {BatchNumber} for keyword search", currentPage);
                    hasMore = false;
                    // Continue with what we have so far instead of failing completely
                    break;
                }
            }
            
            // Filter by keyword (product name) client-side
            stocksWithProducts = new List<(StockDto Stock, ProductDto? Product)>();
            foreach (var stock in allStocks)
            {
                var product = productsDict.ContainsKey(stock.ProductId) ? productsDict[stock.ProductId] : null;
                
                // Filter by keyword (product name)
                if (product == null || 
                    (product.ProductName != null && !product.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                {
                    continue; // Skip this stock if product name doesn't match keyword
                }
                
                stocksWithProducts.Add((stock, product));
            }
            
            // Apply pagination after filtering
            var totalCount = stocksWithProducts.Count;
            var skip = (page - 1) * pageSize;
            var pagedStocks = stocksWithProducts.Skip(skip).Take(pageSize).ToList();
            
            ViewBag.StocksWithProducts = pagedStocks;
            ViewBag.Page = page;
            ViewBag.MaxPage = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));
            
            // Show warning if we hit the batch limit
            if (batchCount >= maxBatches && allStocks.Count >= maxBatches * batchSize)
            {
                TempData["SearchWarning"] = $"Search results may be limited. Showing results from first {allStocks.Count} stocks.";
            }
        }
        else
        {
            // No keyword, use normal pagination
            var filter = new StockFilterDto
            {
                Location = location,
                Keyword = null,
                PageIndex = page,
                PageSize = pageSize,
                OrderBy = "Quantity",
                Descending = false
            };

            var result = await service.GetStocksAsync(filter, cancellationToken);
            ViewBag.Page = page;
            ViewBag.MaxPage = result.Data?.TotalPages ?? 1;
            
            // Load product information for each stock
            stocksWithProducts = new List<(StockDto Stock, ProductDto? Product)>();
            if (result.Success && result.Data != null)
            {
                foreach (var stock in result.Data.Items)
                {
                    var product = productsDict.ContainsKey(stock.ProductId) ? productsDict[stock.ProductId] : null;
                    stocksWithProducts.Add((stock, product));
                }
            }
            
            ViewBag.StocksWithProducts = stocksWithProducts;
        }
        
        ViewBag.Location = location;
        ViewBag.Keyword = keyword;

        // Always return view with the filtered stocks
        // The view will use ViewBag.StocksWithProducts instead of Model
        return View(new PagedResult<StockDto>(stocksWithProducts.Select(s => s.Stock).ToList(), stocksWithProducts.Count, page, pageSize));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Create(Guid? productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var productsResult = await productService.GetAllAsync(cancellationToken);
            if (productsResult.Success && productsResult.Data != null)
            {
                ViewBag.Products = productsResult.Data.OrderBy(p => p.ProductName).ToList();
            }
            else
            {
                ViewBag.Products = new List<ProductDto>();
                ViewBag.ErrorMessage = productsResult.Error?.Message ?? "Error loading products";
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading products for stock creation");
            ViewBag.Products = new List<ProductDto>();
            ViewBag.ErrorMessage = "Error loading products. Please try again.";
        }

        if (productId.HasValue)
        {
            ViewBag.ProductId = productId.Value;
        }
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(CreateStockRequest dto, CancellationToken cancellationToken)
    {
        // Reload products if validation fails
        try
        {
            var productsResult = await productService.GetAllAsync(cancellationToken);
            if (productsResult.Success && productsResult.Data != null)
            {
                ViewBag.Products = productsResult.Data.OrderBy(p => p.ProductName).ToList();
            }
            else
            {
                ViewBag.Products = new List<ProductDto>();
            }
        }
        catch
        {
            ViewBag.Products = new List<ProductDto>();
        }

        if (!ModelState.IsValid)
            return View(dto);

        // Validate ProductId
        if (dto.ProductId == Guid.Empty)
        {
            ModelState.AddModelError("ProductId", "Please select a product");
            return View(dto);
        }

        dto = dto with
        {
            Location = dto.Location?.Trim(),
            Quantity = Math.Max(0, dto.Quantity)
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
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateQuantity(Guid id, int quantity, CancellationToken cancellationToken = default)
    {
        var stock = await service.GetByIdAsync(id, cancellationToken);
        if (!stock.Success || stock.Data == null)
        {
            return Json(new { success = false, message = "Stock not found" });
        }

        var updateRequest = new UpdateStockRequest
        {
            Quantity = quantity,
            Location = stock.Data.Location
        };

        var result = await service.UpdateAsync(id, updateRequest, cancellationToken);
        if (!result.Success)
        {
            return Json(new { success = false, message = result.Error?.Message ?? "Error updating stock" });
        }

        return Json(new { success = true, message = "Quantity updated successfully", newQuantity = quantity });
    }

    [HttpPost]
    [AllowAnonymous]
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


