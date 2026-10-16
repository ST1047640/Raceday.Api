using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class RouteDetail
    {
        [Key]
        public int RouteId { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }

        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;
        public string TerrainType { get; set; } = "Road"; // Road, Trail, Mixed
    }
}