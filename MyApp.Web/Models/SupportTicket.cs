using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(3000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Open";

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "Medium";

        public string? AdminResponse { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public User? User { get; set; }
        public bool AdminViewed { get; set; } = false;

        public bool UserViewed { get; set; } = true;
    }
}