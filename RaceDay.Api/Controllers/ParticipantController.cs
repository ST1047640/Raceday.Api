using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private void AttachAuthToken(HttpClient client)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // 1. Browse upcoming events with filtering
        [HttpGet]
        public async Task<IActionResult> Events(string? search, string? eventType)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            string eventsJson = "[]";

            try
            {
                var response = await client.GetAsync("api/Events");
                if (response.IsSuccessStatusCode)
                {
                    eventsJson = await response.Content.ReadAsStringAsync();
                }
            }
            catch
            {
                eventsJson = "[]";
            }

            ViewBag.Search = search ?? string.Empty;
            ViewBag.EventType = eventType ?? string.Empty;
            ViewBag.EventsJson = eventsJson;
            return View();
        }

        // 2. Full event details & available categories
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            ViewBag.EventId = id;

            try
            {
                var eventResp = await client.GetAsync($"api/Events/{id}");
                ViewBag.EventJson = eventResp.IsSuccessStatusCode ? await eventResp.Content.ReadAsStringAsync() : "{}";
            }
            catch
            {
                ViewBag.EventJson = "{}";
            }

            try
            {
                var catResp = await client.GetAsync($"api/Categories?eventId={id}");
                ViewBag.CategoriesJson = catResp.IsSuccessStatusCode ? await catResp.Content.ReadAsStringAsync() : "[]";
            }
            catch
            {
                ViewBag.CategoriesJson = "[]";
            }

            return View();
        }

        // 3. Enrol in event category
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            AttachAuthToken(client);

            var payload = new
            {
                EventId = eventId,
                CategoryId = categoryId,
                ParticipantEmail = User.Identity?.Name ?? "runner@raceday.com",
                EnrolmentDate = DateTime.UtcNow,
                Status = "Confirmed"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            await client.PostAsync("api/Enrolments", content);

            TempData["Success"] = "You have successfully enrolled for this race!";
            return RedirectToAction("MyEvents");
        }

        // 4. Personal enrolled events
        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            AttachAuthToken(client);

            try
            {
                var response = await client.GetAsync("api/Enrolments/my-enrolments");
                ViewBag.EnrolmentsJson = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "[]";
            }
            catch
            {
                ViewBag.EnrolmentsJson = "[]";
            }

            return View();
        }

        // 5. Personal race history and results
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            AttachAuthToken(client);

            try
            {
                var response = await client.GetAsync("api/Results/my-results");
                ViewBag.ResultsJson = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "[]";
            }
            catch
            {
                ViewBag.ResultsJson = "[]";
            }

            return View();
        }
    }
}