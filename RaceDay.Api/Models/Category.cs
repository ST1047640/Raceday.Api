using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty; // e.g., Under 20, Senior, 10km

        public int EventId { get; set; }
        public Event? Event { get; set; }
    }
}