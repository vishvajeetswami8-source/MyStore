using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.Models
{
    public class SubCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}