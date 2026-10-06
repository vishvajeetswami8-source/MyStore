using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        // Category relationship
        public int? SubCategoryId { get; set; }

        public SubCategory? SubCategory { get; set; }
    }
}