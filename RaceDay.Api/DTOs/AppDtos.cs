using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class RegisterDto
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
        [Required] public string Role { get; set; } = "Participant"; // "Organiser" or "Participant"
        public string? PhoneNumber { get; set; }
    }

    public class LoginDto
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        [Required] public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }

    public class CreateEventDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required] public DateTime Date { get; set; }
        [Required] public string Location { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
        [Required] public string EventType { get; set; } = "Run";
    }

    public class CreateCategoryDto
    {
        [Required] public string CategoryName { get; set; } = string.Empty;
    }

    public class CreateEnrolmentDto
    {
        [Required] public int EventId { get; set; }
        [Required] public int CategoryId { get; set; }
    }

    public class RecordResultDto
    {
        [Required] public int UserId { get; set; }
        [Required] public TimeSpan FinishTime { get; set; }
        [Required] public int Position { get; set; }
    }
}