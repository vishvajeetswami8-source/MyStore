using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending";

        public decimal TotalAmount { get; set; }

        public string? ShippingAddress { get; set; }

        // Navigation properties
        public User? User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}