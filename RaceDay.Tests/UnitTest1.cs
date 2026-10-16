using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using System.Text;
using Xunit;

namespace RaceDay.Tests
{
    public class RaceDayApiTests
    {
        private AppDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
        }

        private static ControllerContext CreateMockControllerContext(int? userId = null, string? role = null)
        {
            var httpContext = new DefaultHttpContext();
            var session = new TestSession();

            if (userId.HasValue)
            {
                session.SetInt32("UserId", userId.Value);
            }

            if (!string.IsNullOrEmpty(role))
            {
                session.SetString("Role", role);
            }

            httpContext.Session = session;

            return new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Register_ValidParticipant_ReturnsCreated()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("RegisterTestDb");
            var controller = new AuthController(context);
            var registerDto = new RegisterDto
            {
                FullName = "Athlete One",
                Email = "athlete@test.com",
                Password = "Password123!",
                Role = "Participant",
                PhoneNumber = "0821234567"
            };

            // Act
            var result = await controller.Register(registerDto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, statusResult.StatusCode);
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("DuplicateEmailDb");
            context.Users.Add(new User
            {
                FullName = "Existing User",
                Email = "duplicate@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = "Participant"
            });
            await context.SaveChangesAsync();

            var controller = new AuthController(context);
            var registerDto = new RegisterDto
            {
                FullName = "New User",
                Email = "duplicate@test.com",
                Password = "Password123!",
                Role = "Participant"
            };

            // Act
            var result = await controller.Register(registerDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreatedAtAction()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("CreateEventOrganiserDb");
            var controller = new EventsController(context)
            {
                ControllerContext = CreateMockControllerContext(userId: 1, role: "Organiser")
            };

            var eventDto = new CreateEventDto
            {
                Name = "City Marathon 2026",
                Description = "Annual city marathon",
                Date = DateTime.UtcNow.AddDays(30),
                Location = "Johannesburg",
                DistanceKm = 42.2,
                EventType = "Run"
            };

            // Act
            var result = await controller.CreateEvent(eventDto);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var createdEvent = Assert.IsType<Event>(actionResult.Value);
            Assert.Equal("City Marathon 2026", createdEvent.Name);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("CreateEventParticipantDb");
            var controller = new EventsController(context)
            {
                ControllerContext = CreateMockControllerContext(userId: 2, role: "Participant")
            };

            var eventDto = new CreateEventDto
            {
                Name = "Unauthorized Event",
                Location = "Sandton",
                Date = DateTime.UtcNow.AddDays(10),
                DistanceKm = 5.0,
                EventType = "Run"
            };

            // Act
            var result = await controller.CreateEvent(eventDto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusResult.StatusCode);
        }

        [Fact]
        public async Task Enrol_ParticipantAlreadyEnroled_ReturnsBadRequest()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("DuplicateEnrolmentDb");
            context.Enrolments.Add(new Enrolment
            {
                UserId = 5,
                EventId = 10,
                CategoryId = 1,
                EnrolmentDate = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context)
            {
                ControllerContext = CreateMockControllerContext(userId: 5, role: "Participant")
            };

            var dto = new CreateEnrolmentDto
            {
                EventId = 10,
                CategoryId = 1
            };

            // Act
            var result = await controller.Enrol(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }

    // In-memory test session provider
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _storage = new();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _storage.Keys;

        public void Clear() => _storage.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _storage.Remove(key);
        public void Set(string key, byte[] value) => _storage[key] = value;
        public bool TryGetValue(string key, out byte[]? value) => _storage.TryGetValue(key, out value);
    }
}