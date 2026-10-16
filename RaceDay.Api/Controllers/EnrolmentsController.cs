using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnrolmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Enrol([FromBody] CreateEnrolmentDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null) return Unauthorized(new { message = "Authentication required." });
            if (role != "Participant") return StatusCode(403, new { message = "Only Participants can enrol in events." });

            var alreadyEnroled = await _context.Enrolments
                .AnyAsync(e => e.UserId == userId.Value && e.EventId == dto.EventId);
            if (alreadyEnroled) return BadRequest(new { message = "You are already enroled in this event." });

            var enrolment = new Enrolment
            {
                UserId = userId.Value,
                EventId = dto.EventId,
                CategoryId = dto.CategoryId,
                EnrolmentDate = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Enrolment successful.", enrolmentId = enrolment.EnrolmentId });
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser") return StatusCode(403, new { message = "Forbidden: Only Organisers can view full enrolment lists." });

            var list = await _context.Enrolments
                .Where(e => e.EventId == eventId)
                .Include(e => e.User)
                .Include(e => e.Category)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.UserId,
                    UserName = e.User!.FullName,
                    Category = e.Category!.CategoryName,
                    e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}