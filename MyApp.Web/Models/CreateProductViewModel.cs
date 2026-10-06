using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MyApp.Web.Models
{
    public class CreateProductViewModel
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Required]
        public int SubCategoryId { get; set; }
    }
}