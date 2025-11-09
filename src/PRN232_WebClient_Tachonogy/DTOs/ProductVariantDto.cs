using System.ComponentModel.DataAnnotations;

namespace PRN232_WebClient_Tachonogy.DTOs;

public record CreateProductVariantRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required, StringLength(255)]
    public string VariantName { get; set; } = null!;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public string? Sku { get; set; }

    public string? ImageUrl { get; set; }
}

public record ProductVariantDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string VariantName { get; set; } = null!;
    public decimal Price { get; set; }
    public string? Sku { get; set; }
    public string? ImageUrl { get; set; }
}

public record UpdateProductVariantRequest
{
    [Required, StringLength(255)]
    public string VariantName { get; set; } = null!;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public string? Sku { get; set; }

    public string? ImageUrl { get; set; }
}

public class ProductVariantFilterDto
{
    public string? Keyword { get; set; }
    public Guid? ProductId { get; set; }
    public int? PageIndex { get; set; } = 1;
    public int? PageSize { get; set; } = 25;
    public string? OrderBy { get; set; } = "Id";
    public bool Descending { get; set; } = false;
}