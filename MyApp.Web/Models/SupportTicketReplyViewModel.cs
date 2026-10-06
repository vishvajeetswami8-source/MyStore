using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.Models
{
    public class SupportTicketReplyViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Status { get; set; } = "In Progress";

        [MaxLength(3000)]
        [Display(Name = "Admin Response")]
        public string? AdminResponse { get; set; }
    }
}