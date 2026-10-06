namespace MyApp.Web.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string OldStatus { get; set; } = string.Empty;

        public string NewStatus { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; } = DateTime.Now;

        public int? ChangedByUserId { get; set; }

        public Order? Order { get; set; }

        public User? ChangedByUser { get; set; }
    }
}