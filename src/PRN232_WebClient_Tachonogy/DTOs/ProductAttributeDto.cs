using System.ComponentModel.DataAnnotations;

namespace PRN232_WebClient_Tachonogy.DTOs;

public record CreateProductAttributeRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required, StringLength(255)]
    public string AttributeName { get; set; } = null!;

    [Required, StringLength(500)]
    public string AttributeValue { get; set; } = null!;
}

public record UpdateProductAttributeRequest
{
    [Required, StringLength(255)]
    public string AttributeName { get; set; } = null!;

    [Required, StringLength(500)]
    public string AttributeValue { get; set; } = null!;
}

public record ProductAttributeDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string AttributeName { get; set; } = null!;
    public string AttributeValue { get; set; } = null!;
}

public class ProductAttributeFilterDto
{
    public string? Keyword { get; set; }
    public Guid? ProductId { get; set; }
    public int? PageIndex { get; set; } = 1;
    public int? PageSize { get; set; } = 25;
    public string? OrderBy { get; set; } = "Id";
    public bool Descending { get; set; } = false;
}