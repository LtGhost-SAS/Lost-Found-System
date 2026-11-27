
using System.ComponentModel.DataAnnotations;

namespace FindBack.Models
{
    public class Item
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string ContactName { get; set; }

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsReturned { get; set; } = false;
    }

}