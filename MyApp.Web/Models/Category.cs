using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<SubCategory> SubCategories { get; set; }
            = new List<SubCategory>();
    }
}