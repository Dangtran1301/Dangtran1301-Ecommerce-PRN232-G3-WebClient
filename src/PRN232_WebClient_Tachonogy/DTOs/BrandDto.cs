using System.ComponentModel.DataAnnotations;

namespace PRN232_WebClient_Tachonogy.DTOs
{
    public record CreateBrandRequest
    {
        [Required(ErrorMessage = "Brand name is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Brand name must between 3 and 255 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Brand name only allows letters and spaces")]
        public string BrandName { get; set; } = null!;
        [Required(ErrorMessage = "Brand description is required")]
        [StringLength(1000, ErrorMessage = "Brand description must not exceed 1000 characters")]
        public string? BrandDescription { get; set; }

        [Required(ErrorMessage = "Brand website URL is required")]
        [StringLength(255, ErrorMessage = "Website URL must not exceed 255 characters")]
        [Url(ErrorMessage = "Website URL must be a valid URL (e.g., https://example.com)")]
        public string? WebsiteUrl { get; set; }
    }
    public record BrandDto
    {
        public Guid Id { get; set; }
        public string BrandName { get; set; } = null!;
        public string? BrandDescription { get; set; }
        public string? WebsiteUrl { get; set; }
    }
    public record UpdateBrandRequest
    {
        [Required(ErrorMessage = "Brand name is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Brand name must between 3 and 255 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Brand name only allows letters and spaces")]
        public string BrandName { get; set; } = null!;
        [Required(ErrorMessage = "Brand description is required")]
        [StringLength(1000, ErrorMessage = "Brand description must not exceed 1000 characters")]
        public string? BrandDescription { get; set; }

        [Required(ErrorMessage = "Brand website URL is required")]
        [StringLength(255, ErrorMessage = "Website URL must not exceed 255 characters")]
        [Url(ErrorMessage = "Website URL must be a valid URL (e.g., https://example.com)")]
        public string? WebsiteUrl { get; set; }
    }

    public class BrandFilterDto
    {
        public string? Keyword { get; set; }
        public int? PageIndex { get; set; } = 1;
        public int? PageSize { get; set; } = 25;
        public string? OrderBy { get; set; } = "CreatedAt";
        public bool Descending { get; set; } = false;
    }
}