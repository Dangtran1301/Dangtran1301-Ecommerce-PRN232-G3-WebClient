using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Services.Interfaces;

namespace PRN232_WebClient_Tachonogy.Controllers;

public class ProductController(
    IProductService productService,
    IBrandService brandService,
    ICategoryService categoryService,
    ILogger<ProductController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? keyword, Guid? brandId, Guid? categoryId, int page = 1, int pageSize = 12, string orderBy = "ProductName", CancellationToken cancellationToken = default)
    {
        var filter = new ProductFilterDto
        {
            Keyword = keyword,
            BrandId = brandId,
            CategoryId = categoryId,
            PageIndex = page,
            PageSize = pageSize,
            OrderBy = orderBy,
            Descending = false
        };

        var result = await productService.GetProductsAsync(filter, cancellationToken);
        ViewBag.Keyword = keyword;
        ViewBag.BrandId = brandId;
        ViewBag.CategoryId = categoryId;
        ViewBag.Page = page;
        ViewBag.MaxPage = result.Data?.TotalPages ?? 1;
        ViewBag.OrderBy = orderBy;

        // Get brands and categories for filters - handle errors gracefully
        try
        {
            var brandsResult = await brandService.GetBrandsAsync(new BrandFilterDto { PageSize = 100 }, cancellationToken);
            ViewBag.Brands = brandsResult.Data?.Items ?? new List<BrandDto>();
        }
        catch
        {
            ViewBag.Brands = new List<BrandDto>();
        }

        try
        {
            var categoriesResult = await categoryService.GetCategoriesAsync(new CategoryFilterDto { PageSize = 100 }, cancellationToken);
            ViewBag.Categories = categoriesResult.Data?.Items ?? new List<CategoryDto>();
        }
        catch
        {
            ViewBag.Categories = new List<CategoryDto>();
        }

        if (result.Success)
            return View(result.Data);

        ViewBag.Error = result.Error?.Message;
        return View(new PagedResult<ProductDto>([], 0, 0, 0));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        // Get brands and categories for dropdown - handle errors gracefully
        try
        {
            var brandsResult = await brandService.GetBrandsAsync(new BrandFilterDto { PageSize = 100 }, cancellationToken);
            ViewBag.Brands = brandsResult.Data?.Items ?? new List<BrandDto>();
        }
        catch
        {
            ViewBag.Brands = new List<BrandDto>();
        }

        try
        {
            var categoriesResult = await categoryService.GetCategoriesAsync(new CategoryFilterDto { PageSize = 100 }, cancellationToken);
            ViewBag.Categories = categoriesResult.Data?.Items ?? new List<CategoryDto>();
        }
        catch
        {
            ViewBag.Categories = new List<CategoryDto>();
        }
        
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // Reload dropdowns - handle errors gracefully
            try
            {
                var brandsResult = await brandService.GetBrandsAsync(new BrandFilterDto { PageSize = 100 }, cancellationToken);
                ViewBag.Brands = brandsResult.Data?.Items ?? new List<BrandDto>();
            }
            catch
            {
                ViewBag.Brands = new List<BrandDto>();
            }

            try
            {
                var categoriesResult = await categoryService.GetCategoriesAsync(new CategoryFilterDto { PageSize = 100 }, cancellationToken);
                ViewBag.Categories = categoriesResult.Data?.Items ?? new List<CategoryDto>();
            }
            catch
            {
                ViewBag.Categories = new List<CategoryDto>();
            }
            return View(dto);
        }

        dto = dto with
        {
            ProductName = dto.ProductName?.Trim() ?? "",
            Description = dto.Description?.Trim(),
            Sku = dto.Sku?.Trim(),
            ImageUrl = dto.ImageUrl?.Trim(),
            Specifications = dto.Specifications?.Trim()
        };

        var result = await productService.CreateAsync(dto, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Error creating product");
            // Reload dropdowns - handle errors gracefully
            try
            {
                var brandsResult = await brandService.GetBrandsAsync(new BrandFilterDto { PageSize = 100 }, cancellationToken);
                ViewBag.Brands = brandsResult.Data?.Items ?? new List<BrandDto>();
            }
            catch
            {
                ViewBag.Brands = new List<BrandDto>();
            }

            try
            {
                var categoriesResult = await categoryService.GetCategoriesAsync(new CategoryFilterDto { PageSize = 100 }, cancellationToken);
                ViewBag.Categories = categoriesResult.Data?.Items ?? new List<CategoryDto>();
            }
            catch
            {
                ViewBag.Categories = new List<CategoryDto>();
            }
            return View(dto);
        }

        TempData["SuccessMessage"] = "Product created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        if (!product.Success || product.Data == null)
            return NotFound();

        // Get brands and categories for dropdown - handle errors gracefully
        try
        {
            var brandsResult = await brandService.GetBrandsAsync(new BrandFilterDto { PageSize = 100 }, cancellationToken);
            ViewBag.Brands = brandsResult.Data?.Items ?? new List<BrandDto>();
        }
        catch
        {
            ViewBag.Brands = new List<BrandDto>();
        }

        try
        {
            var categoriesResult = await categoryService.GetCategoriesAsync(new CategoryFilterDto { PageSize = 100 }, cancellationToken);
            ViewBag.Categories = categoriesResult.Data?.Items ?? new List<CategoryDto>();
        }
        catch
        {
            ViewBag.Categories = new List<CategoryDto>();
        }

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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, UpdateProductRequest dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // Reload dropdowns - handle errors gracefully
            try
            {
                var brandsResult = await brandService.GetBrandsAsync(new BrandFilterDto { PageSize = 100 }, cancellationToken);
                ViewBag.Brands = brandsResult.Data?.Items ?? new List<BrandDto>();
            }
            catch
            {
                ViewBag.Brands = new List<BrandDto>();
            }

            try
            {
                var categoriesResult = await categoryService.GetCategoriesAsync(new CategoryFilterDto { PageSize = 100 }, cancellationToken);
                ViewBag.Categories = categoriesResult.Data?.Items ?? new List<CategoryDto>();
            }
            catch
            {
                ViewBag.Categories = new List<CategoryDto>();
            }
            return View(dto);
        }

        var result = await productService.UpdateAsync(id, dto, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error?.Message ?? "Error updating product");
            // Reload dropdowns - handle errors gracefully
            try
            {
                var brandsResult = await brandService.GetBrandsAsync(new BrandFilterDto { PageSize = 100 }, cancellationToken);
                ViewBag.Brands = brandsResult.Data?.Items ?? new List<BrandDto>();
            }
            catch
            {
                ViewBag.Brands = new List<BrandDto>();
            }

            try
            {
                var categoriesResult = await categoryService.GetCategoriesAsync(new CategoryFilterDto { PageSize = 100 }, cancellationToken);
                ViewBag.Categories = categoriesResult.Data?.Items ?? new List<CategoryDto>();
            }
            catch
            {
                ViewBag.Categories = new List<CategoryDto>();
            }
            return View(dto);
        }

        TempData["SuccessMessage"] = "Product updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
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

        return View(result.Data);
    }
}

