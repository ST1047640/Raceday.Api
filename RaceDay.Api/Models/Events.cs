using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        [Required, MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        public double DistanceKm { get; set; }

        [Required]
        public string EventType { get; set; } = "Run"; // Run, Walk, Cycle

        public int OrganiserId { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}