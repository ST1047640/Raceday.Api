namespace RaceDay.Web.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string EventType { get; set; } = "Running";
        public string BannerUrl { get; set; } = "https://images.unsplash.com/photo-1452626038306-9aae5e071dd3?auto=format&fit=crop&w=800&q=80";
        public DateTime EventDate { get; set; } = DateTime.UtcNow.AddDays(30);
        public List<CategoryViewModel> Categories { get; set; } = new();
    }

    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class RegistrationViewModel
    {
        public int RegistrationId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public int BibNumber { get; set; } = 1042;
        public string Status { get; set; } = "Confirmed";
    }
}