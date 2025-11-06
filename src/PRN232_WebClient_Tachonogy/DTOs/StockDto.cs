using System.ComponentModel.DataAnnotations;

namespace PRN232_WebClient_Tachonogy.DTOs;

public record CreateStockRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [StringLength(255)]
    public string? Location { get; set; }
}

public record StockDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Location { get; set; }
}

public record UpdateStockRequest
{
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [StringLength(255)]
    public string? Location { get; set; }
}

public class StockFilterDto
{
    public Guid? ProductId { get; set; }
    public string? Location { get; set; }
    public int? PageIndex { get; set; } = 1;
    public int? PageSize { get; set; } = 25;
    public string? OrderBy { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = false;
}


