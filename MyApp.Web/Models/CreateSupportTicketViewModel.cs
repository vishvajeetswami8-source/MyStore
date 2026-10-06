using System.ComponentModel.DataAnnotations;

namespace MyApp.Web.Models
{
    public class CreateSupportTicketViewModel
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(3000)]
        [Display(Name = "Describe your issue")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = "Medium";
    }
}