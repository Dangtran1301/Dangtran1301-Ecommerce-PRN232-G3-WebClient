using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.ApiClients;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers;

public class ProductController(
    IProductService productService,
    IBrandService brandService,
    ICategoryService categoryService,
    IProductVariantService productVariantService,
    IProductAttributeService productAttributeService,
    IStockService stockService,
    ODataApiClient odataApiClient,
    ILogger<ProductController> logger) : Controller
{
    private async Task<List<BrandDto>> LoadBrandsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = "odata/ODataBrands?$count=true&$top=100&$skip=0&$orderby=BrandName asc";
            var odataResponse = await odataApiClient.GetAsync<BrandDto>(endpoint, cancellationToken);
            return odataResponse.Value?.ToList() ?? new List<BrandDto>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading brands from OData endpoint");
            return new List<BrandDto>();
        }
    }

    private async Task<List<CategoryDto>> LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = "odata/ODataCategories?$count=true&$top=100&$skip=0&$orderby=CategoryName asc";
            var odataResponse = await odataApiClient.GetAsync<CategoryDto>(endpoint, cancellationToken);
            return odataResponse.Value?.ToList() ?? new List<CategoryDto>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading categories from OData endpoint");
            return new List<CategoryDto>();
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 12, string? sortBy = null, string orderBy = "ProductName", bool descending = false, CancellationToken cancellationToken = default)
    {
        // Handle sortBy from dropdown (Price_ASC, Price_DESC, etc.)
        var actualOrderBy = orderBy;
        var actualDescending = descending;

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortBy == "Price_ASC")
            {
                actualOrderBy = "Price";
                actualDescending = false;
            }
            else if (sortBy == "Price_DESC")
            {
                actualOrderBy = "Price";
                actualDescending = true;
            }
            else
            {
                actualOrderBy = sortBy;
                actualDescending = false;
            }
        }

        var filter = new ProductFilterDto
        {
            Keyword = keyword,
            PageIndex = page,
            PageSize = pageSize,
            OrderBy = actualOrderBy,
            Descending = actualDescending
        };

        var result = await productService.GetProductsAsync(filter, cancellationToken);
        ViewBag.Keyword = keyword;
        ViewBag.Page = page;
        ViewBag.MaxPage = result.Data?.TotalPages ?? 1;
        ViewBag.OrderBy = actualOrderBy;
        ViewBag.Descending = actualDescending;

        if (result.Success)
            return View(result.Data);

        ViewBag.Error = result.Error?.Message;
        return View(new PagedResult<ProductDto>([], 0, 0, 0));
    }

    [AllowAnonymous]
    [HttpGet]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        // Get brands and categories for dropdown - use direct OData endpoint
        ViewBag.Brands = await LoadBrandsAsync(cancellationToken);
        ViewBag.Categories = await LoadCategoriesAsync(cancellationToken);

        // Load all ProductVariants and ProductAttributes for selection
        try
        {
            var variantsResult = await productVariantService.GetProductVariantsAsync(new ProductVariantFilterDto
            {
                PageSize = 1000,
                OrderBy = "VariantName",
                Descending = false
            }, cancellationToken);
            ViewBag.AllProductVariants = variantsResult.Success && variantsResult.Data != null
                ? variantsResult.Data.Items
                : new List<ProductVariantDto>();
        }
        catch
        {
            ViewBag.AllProductVariants = new List<ProductVariantDto>();
        }

        try
        {
            var attributesResult = await productAttributeService.GetProductAttributesAsync(new ProductAttributeFilterDto
            {
                PageSize = 1000,
                OrderBy = "AttributeName",
                Descending = false
            }, cancellationToken);
            ViewBag.AllProductAttributes = attributesResult.Success && attributesResult.Data != null
                ? attributesResult.Data.Items
                : new List<ProductAttributeDto>();
        }
        catch
        {
            ViewBag.AllProductAttributes = new List<ProductAttributeDto>();
        }

        return View();
    }

    [AllowAnonymous]
    [Authorize(Roles = "Admin,Seller")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest dto, CancellationToken cancellationToken)
    {
        // Validate ImageUrl manually if provided
        if (!string.IsNullOrWhiteSpace(dto.ImageUrl) && !Uri.IsWellFormedUriString(dto.ImageUrl, UriKind.Absolute))
        {
            ModelState.AddModelError("ImageUrl", "Invalid image URL format");
        }

        if (!ModelState.IsValid)
        {
            // Log validation errors for debugging
            foreach (var error in ModelState)
            {
                foreach (var errorMessage in error.Value.Errors)
                {
                    logger.LogWarning("Validation error for {Field}: {Error}", error.Key, errorMessage.ErrorMessage);
                }
            }

            // Reload dropdowns - use direct OData endpoint
            ViewBag.Brands = await LoadBrandsAsync(cancellationToken);
            ViewBag.Categories = await LoadCategoriesAsync(cancellationToken);

            // Reload ProductVariants and ProductAttributes
            try
            {
                var variantsResult = await productVariantService.GetProductVariantsAsync(new ProductVariantFilterDto
                {
                    PageSize = 1000,
                    OrderBy = "VariantName",
                    Descending = false
                }, cancellationToken);
                ViewBag.AllProductVariants = variantsResult.Success && variantsResult.Data != null
                    ? variantsResult.Data.Items
                    : new List<ProductVariantDto>();
            }
            catch
            {
                ViewBag.AllProductVariants = new List<ProductVariantDto>();
            }

            try
            {
                var attributesResult = await productAttributeService.GetProductAttributesAsync(new ProductAttributeFilterDto
                {
                    PageSize = 1000,
                    OrderBy = "AttributeName",
                    Descending = false
                }, cancellationToken);
                ViewBag.AllProductAttributes = attributesResult.Success && attributesResult.Data != null
                    ? attributesResult.Data.Items
                    : new List<ProductAttributeDto>();
            }
            catch
            {
                ViewBag.AllProductAttributes = new List<ProductAttributeDto>();
            }

            return View(dto);
        }

        dto = dto with
        {
            ProductName = dto.ProductName?.Trim() ?? "",
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Sku = string.IsNullOrWhiteSpace(dto.Sku) ? null : dto.Sku.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            Specifications = string.IsNullOrWhiteSpace(dto.Specifications) ? null : dto.Specifications.Trim()
        };

        var result = await productService.CreateAsync(dto, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Error creating product");
            // Reload dropdowns - use direct OData endpoint
            ViewBag.Brands = await LoadBrandsAsync(cancellationToken);
            ViewBag.Categories = await LoadCategoriesAsync(cancellationToken);

            // Reload ProductVariants and ProductAttributes
            try
            {
                var variantsResult = await productVariantService.GetProductVariantsAsync(new ProductVariantFilterDto
                {
                    PageSize = 1000,
                    OrderBy = "VariantName",
                    Descending = false
                }, cancellationToken);
                ViewBag.AllProductVariants = variantsResult.Success && variantsResult.Data != null
                    ? variantsResult.Data.Items
                    : new List<ProductVariantDto>();
            }
            catch
            {
                ViewBag.AllProductVariants = new List<ProductVariantDto>();
            }

            try
            {
                var attributesResult = await productAttributeService.GetProductAttributesAsync(new ProductAttributeFilterDto
                {
                    PageSize = 1000,
                    OrderBy = "AttributeName",
                    Descending = false
                }, cancellationToken);
                ViewBag.AllProductAttributes = attributesResult.Success && attributesResult.Data != null
                    ? attributesResult.Data.Items
                    : new List<ProductAttributeDto>();
            }
            catch
            {
                ViewBag.AllProductAttributes = new List<ProductAttributeDto>();
            }

            return View(dto);
        }

        // Product created successfully, now create ProductVariants and ProductAttributes if provided
        if (result.Data == null)
        {
            logger.LogError("Product created but result.Data is null");
            TempData["ErrorMessage"] = "Product was created but we couldn't retrieve its ID. Please refresh the page.";
            return RedirectToAction(nameof(Index));
        }

        var productId = result.Data.Id;
        var errors = new List<string>();

        // Associate selected ProductVariants with this product (copy them to this product)
        if (dto.SelectedVariantIds != null && dto.SelectedVariantIds.Any())
        {
            foreach (var variantId in dto.SelectedVariantIds)
            {
                try
                {
                    // Get the variant to check if it exists and get its current data
                    var variantResult = await productVariantService.GetByIdAsync(variantId, cancellationToken);
                    if (variantResult.Success && variantResult.Data != null)
                    {
                        // Create a copy of the variant for this product (don't delete the original)
                        var createDto = new CreateProductVariantRequest
                        {
                            ProductId = productId,
                            VariantName = variantResult.Data.VariantName,
                            Price = variantResult.Data.Price,
                            Sku = variantResult.Data.Sku,
                            ImageUrl = variantResult.Data.ImageUrl
                        };

                        var createResult = await productVariantService.CreateAsync(createDto, cancellationToken);
                        if (!createResult.Success)
                        {
                            errors.Add($"Failed to associate variant '{variantResult.Data.VariantName}': {createResult.Error?.Message}");
                            logger.LogWarning("Failed to create ProductVariant copy for Product {ProductId}: {Error}",
                                productId, createResult.Error?.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to associate variant {variantId}: {ex.Message}");
                    logger.LogWarning(ex, "Failed to associate ProductVariant {VariantId} with Product {ProductId}", variantId, productId);
                }
            }
        }

        // Associate selected ProductAttributes with this product (copy them to this product)
        if (dto.SelectedAttributeIds != null && dto.SelectedAttributeIds.Any())
        {
            foreach (var attributeId in dto.SelectedAttributeIds)
            {
                try
                {
                    // Get the attribute to check if it exists and get its current data
                    var attributeResult = await productAttributeService.GetByIdAsync(attributeId, cancellationToken);
                    if (attributeResult.Success && attributeResult.Data != null)
                    {
                        // Create a copy of the attribute for this product (don't delete the original)
                        var createDto = new CreateProductAttributeRequest
                        {
                            ProductId = productId,
                            AttributeName = attributeResult.Data.AttributeName,
                            AttributeValue = attributeResult.Data.AttributeValue
                        };

                        var createResult = await productAttributeService.CreateAsync(createDto, cancellationToken);
                        if (!createResult.Success)
                        {
                            errors.Add($"Failed to associate attribute '{attributeResult.Data.AttributeName}': {createResult.Error?.Message}");
                            logger.LogWarning("Failed to create ProductAttribute copy for Product {ProductId}: {Error}",
                                productId, createResult.Error?.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to associate attribute {attributeId}: {ex.Message}");
                    logger.LogWarning(ex, "Failed to associate ProductAttribute {AttributeId} with Product {ProductId}", attributeId, productId);
                }
            }
        }

        // Create or Update Stock (Business Logic: Only create/update if quantity > 0 or location is provided)
        if (dto.Stock != null && (dto.Stock.Quantity > 0 || !string.IsNullOrWhiteSpace(dto.Stock.Location)))
        {
            // Check if stock already exists for this product
            var existingStockResult = await stockService.GetStocksAsync(new StockFilterDto
            {
                ProductId = productId,
                PageSize = 1
            }, cancellationToken);

            var existingStock = existingStockResult.Success && existingStockResult.Data != null && existingStockResult.Data.Items.Any()
                ? existingStockResult.Data.Items.First()
                : null;

            if (existingStock != null)
            {
                // Update existing stock
                var updateStockDto = new UpdateStockRequest
                {
                    Quantity = dto.Stock.Quantity,
                    Location = dto.Stock.Location?.Trim()
                };

                var stockUpdateResult = await stockService.UpdateAsync(existingStock.Id, updateStockDto, cancellationToken);
                if (!stockUpdateResult.Success)
                {
                    errors.Add($"Failed to update stock: {stockUpdateResult.Error?.Message}");
                    logger.LogWarning("Failed to update Stock for Product {ProductId}: {Error}",
                        productId, stockUpdateResult.Error?.Message);
                }
            }
            else
            {
                // Create new stock
                var stockDto = new CreateStockRequest
                {
                    ProductId = productId,
                    Quantity = dto.Stock.Quantity,
                    Location = dto.Stock.Location?.Trim()
                };

                var stockResult = await stockService.CreateAsync(stockDto, cancellationToken);
                if (!stockResult.Success)
                {
                    errors.Add($"Failed to create stock: {stockResult.Error?.Message}");
                    logger.LogWarning("Failed to create Stock for Product {ProductId}: {Error}",
                        productId, stockResult.Error?.Message);
                }
            }
        }

        if (errors.Any())
        {
            TempData["WarningMessage"] = $"Product created successfully, but some variants/attributes/stock failed: {string.Join("; ", errors)}";
        }
        else
        {
            TempData["SuccessMessage"] = "Product created successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    [Authorize(Roles = "Admin,Seller")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        if (!product.Success || product.Data == null)
            return NotFound();

        // Get brands and categories for dropdown - use direct OData endpoint
        ViewBag.Brands = await LoadBrandsAsync(cancellationToken);
        ViewBag.Categories = await LoadCategoriesAsync(cancellationToken);

        // Load ProductVariants and ProductAttributes for this product
        try
        {
            var variantsResult = await productVariantService.GetProductVariantsAsync(new ProductVariantFilterDto
            {
                ProductId = id,
                PageSize = 100
            }, cancellationToken);
            ViewBag.ProductVariants = variantsResult.Success && variantsResult.Data != null
                ? variantsResult.Data.Items
                : new List<ProductVariantDto>();
        }
        catch
        {
            ViewBag.ProductVariants = new List<ProductVariantDto>();
        }

        try
        {
            var attributesResult = await productAttributeService.GetProductAttributesAsync(new ProductAttributeFilterDto
            {
                ProductId = id,
                PageSize = 100
            }, cancellationToken);
            ViewBag.ProductAttributes = attributesResult.Success && attributesResult.Data != null
                ? attributesResult.Data.Items
                : new List<ProductAttributeDto>();
        }
        catch
        {
            ViewBag.ProductAttributes = new List<ProductAttributeDto>();
        }

        // Load all ProductVariants and ProductAttributes for selection
        try
        {
            var allVariantsResult = await productVariantService.GetProductVariantsAsync(new ProductVariantFilterDto
            {
                PageSize = 1000,
                OrderBy = "VariantName",
                Descending = false
            }, cancellationToken);
            ViewBag.AllProductVariants = allVariantsResult.Success && allVariantsResult.Data != null
                ? allVariantsResult.Data.Items
                : new List<ProductVariantDto>();
        }
        catch
        {
            ViewBag.AllProductVariants = new List<ProductVariantDto>();
        }

        try
        {
            var allAttributesResult = await productAttributeService.GetProductAttributesAsync(new ProductAttributeFilterDto
            {
                PageSize = 1000,
                OrderBy = "AttributeName",
                Descending = false
            }, cancellationToken);
            ViewBag.AllProductAttributes = allAttributesResult.Success && allAttributesResult.Data != null
                ? allAttributesResult.Data.Items
                : new List<ProductAttributeDto>();
        }
        catch
        {
            ViewBag.AllProductAttributes = new List<ProductAttributeDto>();
        }

        // Load existing Stock for this product
        try
        {
            var stockResult = await stockService.GetStocksAsync(new StockFilterDto
            {
                ProductId = id,
                PageSize = 1
            }, cancellationToken);
            ViewBag.ExistingStock = stockResult.Success && stockResult.Data != null && stockResult.Data.Items.Any()
                ? stockResult.Data.Items.First()
                : null;
        }
        catch
        {
            ViewBag.ExistingStock = null;
        }

        ViewBag.ProductId = id;

        var dto = new UpdateProductRequest
        {
            ProductName = product.Data.ProductName.Trim(),
            Description = product.Data.Description?.Trim(),
            Price = product.Data.Price,
            Sku = product.Data.Sku?.Trim(),
            ImageUrl = product.Data.ImageUrl?.Trim(),
            Specifications = product.Data.Specifications?.Trim(),
            BrandId = product.Data.BrandId,
            CategoryId = product.Data.CategoryId
        };
        return View(dto);
    }

    [AllowAnonymous]
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, UpdateProductRequest dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // Reload dropdowns - use direct OData endpoint
            ViewBag.Brands = await LoadBrandsAsync(cancellationToken);
            ViewBag.Categories = await LoadCategoriesAsync(cancellationToken);
            return View(dto);
        }

        var result = await productService.UpdateAsync(id, dto, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Error updating product");
            // Reload dropdowns - use direct OData endpoint
            ViewBag.Brands = await LoadBrandsAsync(cancellationToken);
            ViewBag.Categories = await LoadCategoriesAsync(cancellationToken);

            // Reload ProductVariants and ProductAttributes
            try
            {
                var variantsResult = await productVariantService.GetProductVariantsAsync(new ProductVariantFilterDto
                {
                    PageSize = 1000,
                    OrderBy = "VariantName",
                    Descending = false
                }, cancellationToken);
                ViewBag.AllProductVariants = variantsResult.Success && variantsResult.Data != null
                    ? variantsResult.Data.Items
                    : new List<ProductVariantDto>();
            }
            catch
            {
                ViewBag.AllProductVariants = new List<ProductVariantDto>();
            }

            try
            {
                var attributesResult = await productAttributeService.GetProductAttributesAsync(new ProductAttributeFilterDto
                {
                    PageSize = 1000,
                    OrderBy = "AttributeName",
                    Descending = false
                }, cancellationToken);
                ViewBag.AllProductAttributes = attributesResult.Success && attributesResult.Data != null
                    ? attributesResult.Data.Items
                    : new List<ProductAttributeDto>();
            }
            catch
            {
                ViewBag.AllProductAttributes = new List<ProductAttributeDto>();
            }

            return View(dto);
        }

        // Product updated successfully, now create ProductVariants and ProductAttributes if provided
        var errors = new List<string>();

        // Note: We use the id parameter for ProductId since we're updating an existing product

        // Associate selected ProductVariants with this product (copy them to this product)
        if (dto.SelectedVariantIds != null && dto.SelectedVariantIds.Any())
        {
            foreach (var variantId in dto.SelectedVariantIds)
            {
                try
                {
                    // Get the variant to check if it exists and get its current data
                    var variantResult = await productVariantService.GetByIdAsync(variantId, cancellationToken);
                    if (variantResult.Success && variantResult.Data != null)
                    {
                        // Check if this variant already belongs to this product
                        if (variantResult.Data.ProductId == id)
                        {
                            continue; // Already associated, skip
                        }

                        // Create a copy of the variant for this product (don't delete the original)
                        var createDto = new CreateProductVariantRequest
                        {
                            ProductId = id,
                            VariantName = variantResult.Data.VariantName,
                            Price = variantResult.Data.Price,
                            Sku = variantResult.Data.Sku,
                            ImageUrl = variantResult.Data.ImageUrl
                        };

                        var createResult = await productVariantService.CreateAsync(createDto, cancellationToken);
                        if (!createResult.Success)
                        {
                            errors.Add($"Failed to associate variant '{variantResult.Data.VariantName}': {createResult.Error?.Message}");
                            logger.LogWarning("Failed to create ProductVariant copy for Product {ProductId}: {Error}",
                                id, createResult.Error?.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to associate variant {variantId}: {ex.Message}");
                    logger.LogWarning(ex, "Failed to associate ProductVariant {VariantId} with Product {ProductId}", variantId, id);
                }
            }
        }

        // Associate selected ProductAttributes with this product (copy them to this product)
        if (dto.SelectedAttributeIds != null && dto.SelectedAttributeIds.Any())
        {
            foreach (var attributeId in dto.SelectedAttributeIds)
            {
                try
                {
                    // Get the attribute to check if it exists and get its current data
                    var attributeResult = await productAttributeService.GetByIdAsync(attributeId, cancellationToken);
                    if (attributeResult.Success && attributeResult.Data != null)
                    {
                        // Check if this attribute already belongs to this product
                        if (attributeResult.Data.ProductId == id)
                        {
                            continue; // Already associated, skip
                        }

                        // Create a copy of the attribute for this product (don't delete the original)
                        var createDto = new CreateProductAttributeRequest
                        {
                            ProductId = id,
                            AttributeName = attributeResult.Data.AttributeName,
                            AttributeValue = attributeResult.Data.AttributeValue
                        };

                        var createResult = await productAttributeService.CreateAsync(createDto, cancellationToken);
                        if (!createResult.Success)
                        {
                            errors.Add($"Failed to associate attribute '{attributeResult.Data.AttributeName}': {createResult.Error?.Message}");
                            logger.LogWarning("Failed to create ProductAttribute copy for Product {ProductId}: {Error}",
                                id, createResult.Error?.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to associate attribute {attributeId}: {ex.Message}");
                    logger.LogWarning(ex, "Failed to associate ProductAttribute {AttributeId} with Product {ProductId}", attributeId, id);
                }
            }
        }

        // Update Stock (Business Logic: Always update stock if provided, even if quantity is 0)
        if (dto.Stock != null)
        {
            // Check if stock exists for this product
            var existingStockResult = await stockService.GetStocksAsync(new StockFilterDto
            {
                ProductId = id,
                PageSize = 1
            }, cancellationToken);

            var existingStock = existingStockResult.Success && existingStockResult.Data != null && existingStockResult.Data.Items.Any()
                ? existingStockResult.Data.Items.First()
                : null;

            if (existingStock != null)
            {
                // Update existing stock (allow quantity = 0 for out of stock)
                var updateStockDto = new UpdateStockRequest
                {
                    Quantity = dto.Stock.Quantity,
                    Location = dto.Stock.Location?.Trim()
                };

                var stockUpdateResult = await stockService.UpdateAsync(existingStock.Id, updateStockDto, cancellationToken);
                if (!stockUpdateResult.Success)
                {
                    errors.Add($"Failed to update stock: {stockUpdateResult.Error?.Message}");
                    logger.LogWarning("Failed to update Stock for Product {ProductId}: {Error}",
                        id, stockUpdateResult.Error?.Message);
                }
            }
            else
            {
                // Create new stock if it doesn't exist (only if quantity > 0 or location provided)
                if (dto.Stock.Quantity > 0 || !string.IsNullOrWhiteSpace(dto.Stock.Location))
                {
                    var stockDto = new CreateStockRequest
                    {
                        ProductId = id,
                        Quantity = dto.Stock.Quantity,
                        Location = dto.Stock.Location?.Trim()
                    };

                    var stockResult = await stockService.CreateAsync(stockDto, cancellationToken);
                    if (!stockResult.Success)
                    {
                        errors.Add($"Failed to create stock: {stockResult.Error?.Message}");
                        logger.LogWarning("Failed to create Stock for Product {ProductId}: {Error}",
                            id, stockResult.Error?.Message);
                    }
                }
            }
        }

        if (errors.Any())
        {
            TempData["WarningMessage"] = $"Product updated successfully, but some variants/attributes/stock failed: {string.Join("; ", errors)}";
        }
        else
        {
            TempData["SuccessMessage"] = "Product updated successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await productService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            TempData["ErrorMessage"] = result.Error?.Message ?? "Error deleting product";
        else
            TempData["SuccessMessage"] = "Product deleted successfully!";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        var result = await productService.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToAction(nameof(Index));
        }

        // Load ProductVariants for this product
        try
        {
            var variantsResult = await productVariantService.GetProductVariantsAsync(new ProductVariantFilterDto
            {
                ProductId = id,
                PageSize = 100
            }, cancellationToken);
            ViewBag.ProductVariants = variantsResult.Success && variantsResult.Data != null
                ? variantsResult.Data.Items
                : new List<ProductVariantDto>();
        }
        catch
        {
            ViewBag.ProductVariants = new List<ProductVariantDto>();
        }

        // Load ProductAttributes for this product
        try
        {
            var attributesResult = await productAttributeService.GetProductAttributesAsync(new ProductAttributeFilterDto
            {
                ProductId = id,
                PageSize = 100
            }, cancellationToken);
            ViewBag.ProductAttributes = attributesResult.Success && attributesResult.Data != null
                ? attributesResult.Data.Items
                : new List<ProductAttributeDto>();
        }
        catch
        {
            ViewBag.ProductAttributes = new List<ProductAttributeDto>();
        }

        // Load Stock for this product
        try
        {
            var stockResult = await stockService.GetStocksAsync(new StockFilterDto
            {
                ProductId = id,
                PageSize = 1
            }, cancellationToken);
            ViewBag.Stock = stockResult.Success && stockResult.Data != null && stockResult.Data.Items.Any()
                ? stockResult.Data.Items.First()
                : null;
        }
        catch
        {
            ViewBag.Stock = null;
        }

        return View(result.Data);
    }
}