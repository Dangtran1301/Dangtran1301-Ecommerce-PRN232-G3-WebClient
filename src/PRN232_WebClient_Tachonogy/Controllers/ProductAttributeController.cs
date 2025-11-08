using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.ApiClients;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers;

public class ProductAttributeController(
    IProductAttributeService service,
    IProductService productService,
    ODataApiClient odataApiClient,
    ILogger<ProductAttributeController> logger) : Controller
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
            OrderBy = "Id",
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
    public async Task<IActionResult> Create(CreateProductAttributeRequest dto, CancellationToken cancellationToken)
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
            AttributeName = dto.AttributeName?.Trim() ?? "",
            AttributeValue = dto.AttributeValue?.Trim() ?? ""
        };
        
        // Re-validate after trimming
        if (string.IsNullOrWhiteSpace(dto.AttributeName))
        {
            ModelState.AddModelError("AttributeName", "Attribute name is required");
        }
        
        if (string.IsNullOrWhiteSpace(dto.AttributeValue))
        {
            ModelState.AddModelError("AttributeValue", "Attribute value is required");
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

        logger.LogInformation("Creating product attribute: ProductId={ProductId}, AttributeName={AttributeName}, AttributeValue={AttributeValue}", 
            dto.ProductId, dto.AttributeName, dto.AttributeValue);
        
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
            
            var errorMessage = result.Error?.Message ?? "Error creating product attribute";
            ModelState.AddModelError("", errorMessage);
            logger.LogError("Failed to create product attribute: {Error}. ProductId={ProductId}, AttributeName={AttributeName}", 
                errorMessage, dto.ProductId, dto.AttributeName);
            return View(dto);
        }

        logger.LogInformation("Product attribute created successfully: ProductId={ProductId}, AttributeName={AttributeName}", 
            dto.ProductId, dto.AttributeName);
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


