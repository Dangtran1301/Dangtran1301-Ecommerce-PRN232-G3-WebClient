using System.ComponentModel.DataAnnotations;

namespace PRN232_WebClient_Tachonogy.DTOs
{
    public record OrderDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public record OrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public record CreateOrderItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public record CreateOrderRequest
    {
        public Guid CustomerId { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public record UpdateOrderRequest
    {
        public Guid Id { get; set; }
        [Required]
        public int? Status { get; set; }
    }

    public class OrderFilterDto
    {
        public Guid? CustomerId { get; set; }
        public string? Status { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}