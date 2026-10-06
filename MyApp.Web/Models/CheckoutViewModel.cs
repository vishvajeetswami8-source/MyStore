using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.Models
{
    public class CheckoutViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string Pincode { get; set; } = string.Empty;

        public List<CheckoutItemViewModel> Items { get; set; }
            = new List<CheckoutItemViewModel>();

        public decimal TotalAmount =>
            Items.Sum(x => x.Total);
    }
}