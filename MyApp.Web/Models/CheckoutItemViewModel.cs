namespace MyApp.Web.Models
{
    public class CheckoutItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal Total =>
            UnitPrice * Quantity;
    }
}