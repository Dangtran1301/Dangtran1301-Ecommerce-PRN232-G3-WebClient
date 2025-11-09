using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PRN232_WebClient_Tachonogy.DTOs
{
    public record CreateCategoryRequest
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Category name must between 3 and 255 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Category name only allows letters and spaces")]
        public string CategoryName { get; set; } = null!;

        [Required(ErrorMessage = "Category description is required")]
        [StringLength(1000, ErrorMessage = "Category description must not exceed 1000 characters")]
        public string? CategoryDescription { get; set; }
    }
    public record CategoryDto
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? CategoryDescription { get; set; }
    }
    public record UpdateCategoryRequest
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Category name must between 3 and 255 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Category name only allows letters and spaces")]
        public string CategoryName { get; set; } = null!;

        [Required(ErrorMessage = "Category description is required")]
        [StringLength(1000, ErrorMessage = "Category description must not exceed 1000 characters")]
        public string? CategoryDescription { get; set; }
    }

    public class CategoryFilterDto
    {
        public string? Keyword { get; set; }
        public int? PageIndex { get; set; } = 1;
        public int? PageSize { get; set; } = 25;
        public string? OrderBy { get; set; } = "CreatedAt";
        public bool Descending { get; set; } = false;
    }

    public class ODataResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string? OdataContext { get; set; }

        [JsonPropertyName("value")]
        public List<T>? Value { get; set; }

        [JsonPropertyName("@odata.count")]
        public int? Count { get; set; }
    }
}