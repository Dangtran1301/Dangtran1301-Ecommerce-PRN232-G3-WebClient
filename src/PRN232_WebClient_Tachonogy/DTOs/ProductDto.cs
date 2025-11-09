using System.ComponentModel.DataAnnotations;

namespace PRN232_WebClient_Tachonogy.DTOs;

public record CreateProductRequest
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(255, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 255 characters")]
    public string ProductName { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [StringLength(255)]
    public string? Sku { get; set; }

    public string? ImageUrl { get; set; }

    public string? Specifications { get; set; }

    [Required(ErrorMessage = "Brand is required")]
    public Guid BrandId { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public Guid CategoryId { get; set; }

    // Selected Variant IDs to associate with the product (existing variants)
    public List<Guid>? SelectedVariantIds { get; set; }

    // Selected Attribute IDs to associate with the product (existing attributes)
    public List<Guid>? SelectedAttributeIds { get; set; }

    // Stock information to create along with the product
    public CreateStockInlineRequest? Stock { get; set; }
}

public record CreateStockInlineRequest
{
    public int Quantity { get; set; } = 0;

    [StringLength(255)]
    public string? Location { get; set; }
}

public record CreateProductVariantInlineRequest
{
    [StringLength(255)]
    public string? VariantName { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }

    [StringLength(255)]
    public string? Sku { get; set; }

    [Url(ErrorMessage = "Invalid image URL format")]
    public string? ImageUrl { get; set; }
}

public record CreateProductAttributeInlineRequest
{
    [StringLength(255)]
    public string? AttributeName { get; set; }

    [StringLength(500)]
    public string? AttributeValue { get; set; }
}

public record ProductDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Sku { get; set; }
    public string? ImageUrl { get; set; }
    public string? Specifications { get; set; }
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
}

public record UpdateProductRequest
{
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string ProductName { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [StringLength(255)]
    public string? Sku { get; set; }

    [Url]
    public string? ImageUrl { get; set; }

    public string? Specifications { get; set; }

    [Required]
    public Guid BrandId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    // Selected Variant IDs to associate with the product (existing variants)
    public List<Guid>? SelectedVariantIds { get; set; }

    // Selected Attribute IDs to associate with the product (existing attributes)
    public List<Guid>? SelectedAttributeIds { get; set; }

    // Stock information to update along with the product
    public UpdateStockInlineRequest? Stock { get; set; }
}

public record UpdateStockInlineRequest
{
    public int Quantity { get; set; } = 0;

    [StringLength(255)]
    public string? Location { get; set; }
}

public class ProductFilterDto
{
    public string? Keyword { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? CategoryId { get; set; }
    public int? PageIndex { get; set; } = 1;
    public int? PageSize { get; set; } = 25;
    public string? OrderBy { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = false;
}