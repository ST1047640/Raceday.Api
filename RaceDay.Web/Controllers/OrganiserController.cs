using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace RaceDay.Web.Controllers
{
    [Authorize(Roles = "Organiser")]
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrganiserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Dashboard()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var response = await client.GetAsync("api/Events");

            int totalEvents = 0;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var events = JsonSerializer.Deserialize<List<dynamic>>(content);
                totalEvents = events?.Count ?? 0;
            }

            ViewBag.TotalEvents = totalEvents;
            return View();
        }

        public async Task<IActionResult> Events()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var response = await client.GetAsync("api/Events");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.EventsJson = content;
            }
            return View();
        }
    }
}