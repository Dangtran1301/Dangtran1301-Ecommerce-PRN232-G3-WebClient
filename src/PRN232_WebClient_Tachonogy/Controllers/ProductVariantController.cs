using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.ApiClients;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers;

public class ProductVariantController(
    IProductVariantService service,
    IProductService productService,
    ODataApiClient odataApiClient,
    ILogger<ProductVariantController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? keyword, Guid? productId, int page = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        var filter = new ProductVariantFilterDto
        {
            Keyword = keyword,
            ProductId = productId,
            PageIndex = page,
            PageSize = pageSize, // Increase page size to get all variants for grouping
            OrderBy = "Id",
            Descending = false
        };

        var result = await service.GetProductVariantsAsync(filter, cancellationToken);
        ViewBag.Keyword = keyword;
        ViewBag.ProductId = productId;
        ViewBag.Page = page;
        ViewBag.MaxPage = result.Data?.TotalPages ?? 1;

        // Load all products to map ProductId to ProductName
        var productsDict = new Dictionary<Guid, ProductDto>();
        try
        {
            var productsResult = await productService.GetAllAsync(cancellationToken);
            if (productsResult.Success && productsResult.Data != null)
            {
                foreach (var product in productsResult.Data)
                {
                    productsDict[product.Id] = product;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading products for variant index");
        }

        ViewBag.ProductsDict = productsDict;

        if (result.Success)
            return View(result.Data);

        ViewBag.Error = result.Error?.Message;
        return View(new PagedResult<ProductVariantDto>([], 0, 0, 0));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Create(Guid? productId, CancellationToken cancellationToken = default)
    {
        // Load all products for dropdown using REST API
        try
        {
            var productsResult = await productService.GetAllAsync(cancellationToken);
            
            if (productsResult.Success && productsResult.Data != null)
            {
                ViewBag.Products = productsResult.Data.ToList();
                logger.LogInformation("Loaded {Count} products for dropdown", productsResult.Data.Count);
            }
            else
            {
                ViewBag.Products = new List<ProductDto>();
                ViewBag.ErrorMessage = productsResult.Error?.Message ?? "Failed to load products. Please try again.";
                logger.LogWarning("Failed to load products: {Error}", productsResult.Error?.Message ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            ViewBag.Products = new List<ProductDto>();
            ViewBag.ErrorMessage = $"Error loading products: {ex.Message}";
            logger.LogError(ex, "Exception while loading products for dropdown");
        }

        if (productId.HasValue)
        {
            ViewBag.ProductId = productId.Value;
        }
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductVariantRequest dto, CancellationToken cancellationToken)
    {
        // Validate ProductId
        if (dto.ProductId == Guid.Empty)
        {
            ModelState.AddModelError("ProductId", "Please select a product.");
        }
        
        // Reload products for dropdown if validation fails
        if (!ModelState.IsValid)
        {
            try
            {
                var productsResult = await productService.GetAllAsync(cancellationToken);
                
                if (productsResult.Success && productsResult.Data != null)
                {
                    ViewBag.Products = productsResult.Data.ToList();
                }
                else
                {
                    ViewBag.Products = new List<ProductDto>();
                    ViewBag.ErrorMessage = productsResult.Error?.Message ?? "Failed to load products.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Products = new List<ProductDto>();
                ViewBag.ErrorMessage = $"Error loading products: {ex.Message}";
                logger.LogError(ex, "Exception while reloading products");
            }
            return View(dto);
        }

        // Trim and validate input
        dto = dto with
        {
            VariantName = dto.VariantName?.Trim() ?? "",
            Sku = string.IsNullOrWhiteSpace(dto.Sku) ? null : dto.Sku.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim()
        };
        
        // Validate ImageUrl format if provided
        if (!string.IsNullOrWhiteSpace(dto.ImageUrl) && !Uri.IsWellFormedUriString(dto.ImageUrl, UriKind.Absolute))
        {
            ModelState.AddModelError("ImageUrl", "Invalid image URL format");
        }
        
        // Re-validate after trimming
        if (string.IsNullOrWhiteSpace(dto.VariantName))
        {
            ModelState.AddModelError("VariantName", "Variant name is required");
        }
        
        if (dto.Price <= 0)
        {
            ModelState.AddModelError("Price", "Price must be greater than 0");
        }
        
        if (!ModelState.IsValid)
        {
            // Reload products for dropdown
            try
            {
                var productsResult = await productService.GetAllAsync(cancellationToken);
                if (productsResult.Success && productsResult.Data != null)
                {
                    ViewBag.Products = productsResult.Data.ToList();
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
            return View(dto);
        }

        logger.LogInformation("Creating product variant: ProductId={ProductId}, VariantName={VariantName}, Price={Price}", 
            dto.ProductId, dto.VariantName, dto.Price);
        
        var result = await service.CreateAsync(dto, cancellationToken);
        if (!result.Success)
        {
            // Reload products for dropdown
            try
            {
                var productsResult = await productService.GetAllAsync(cancellationToken);
                if (productsResult.Success && productsResult.Data != null)
                {
                    ViewBag.Products = productsResult.Data.ToList();
                }
                else
                {
                    ViewBag.Products = new List<ProductDto>();
                    ViewBag.ErrorMessage = productsResult.Error?.Message ?? "Failed to load products.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Products = new List<ProductDto>();
                ViewBag.ErrorMessage = $"Error loading products: {ex.Message}";
                logger.LogError(ex, "Exception while reloading products");
            }
            
            var errorMessage = result.Error?.Message ?? "Error creating product variant";
            ModelState.AddModelError("", errorMessage);
            logger.LogError("Failed to create product variant: {Error}. ProductId={ProductId}, VariantName={VariantName}", 
                errorMessage, dto.ProductId, dto.VariantName);
            return View(dto);
        }

        logger.LogInformation("Product variant created successfully: ProductId={ProductId}, VariantName={VariantName}", 
            dto.ProductId, dto.VariantName);
        TempData["SuccessMessage"] = "Product variant created successfully!";
        return RedirectToAction(nameof(Index));
    }

    //[Authorize(Roles = "Admin")]

    [HttpGet]
    [AllowAnonymous]
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
    [AllowAnonymous]
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


