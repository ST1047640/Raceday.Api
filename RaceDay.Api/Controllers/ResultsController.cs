using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResultsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("event/{eventId}")]
        public async Task<IActionResult> RecordResult(int eventId, [FromBody] RecordResultDto dto)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser") return StatusCode(403, new { message = "Forbidden: Only Organisers can record results." });

            var result = new Result
            {
                EventId = eventId,
                UserId = dto.UserId,
                FinishTime = dto.FinishTime,
                Position = dto.Position
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, result);
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var results = await _context.Results
                .Where(r => r.EventId == eventId)
                .OrderBy(r => r.Position)
                .Include(r => r.User)
                .Select(r => new
                {
                    r.Position,
                    UserName = r.User!.FullName,
                    FinishTime = r.FinishTime.ToString(@"hh\:mm\:ss")
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}