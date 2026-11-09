using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
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

        private async Task AttachAuthHeaderAsync(HttpClient client)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                var loginPayload = new
                {
                    Email = User.Identity?.Name ?? "admin2026@raceday.com",
                    Password = "RaceDay@2026!"
                };

                var loginContent = new StringContent(
                    JsonSerializer.Serialize(loginPayload),
                    Encoding.UTF8,
                    "application/json"
                );

                var authResponse = await client.PostAsync("api/Auth/login", loginContent);
                if (authResponse.IsSuccessStatusCode)
                {
                    var authJson = await authResponse.Content.ReadAsStringAsync();
                    using var authDoc = JsonDocument.Parse(authJson);

                    if (authDoc.RootElement.TryGetProperty("token", out var tProp) ||
                        authDoc.RootElement.TryGetProperty("tokenString", out tProp) ||
                        authDoc.RootElement.TryGetProperty("accessToken", out tProp))
                    {
                        token = tProp.GetString();
                        if (!string.IsNullOrEmpty(token))
                        {
                            HttpContext.Session.SetString("JWToken", token);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            try
            {
                var response = await client.GetAsync("api/Events");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    ViewBag.TotalEvents = doc.RootElement.GetArrayLength();
                }
                else
                {
                    ViewBag.TotalEvents = 0;
                }
            }
            catch
            {
                ViewBag.TotalEvents = 0;
            }

            ViewBag.TotalParticipants = 0;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Events()
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            try
            {
                var response = await client.GetAsync("api/Events");
                if (response.IsSuccessStatusCode)
                {
                    ViewBag.EventsJson = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    ViewBag.EventsJson = "[]";
                }
            }
            catch
            {
                ViewBag.EventsJson = "[]";
            }

            return View();
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(
            string name,
            DateTime date,
            string location,
            string distance,
            string eventType,
            string description,
            IFormFile? bannerImage)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(string.Empty, "Event Name is required.");
                return View();
            }

            var client = _httpClientFactory.CreateClient("RaceDayApi");
            await AttachAuthHeaderAsync(client);

            string bannerUrl = string.Empty;

            // 1. Upload Banner Image to Azure Blob Storage via RaceDay.Api
            if (bannerImage != null && bannerImage.Length > 0)
            {
                try
                {
                    using var multipartContent = new MultipartFormDataContent();
                    using var stream = bannerImage.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(bannerImage.ContentType);
                    multipartContent.Add(fileContent, "file", bannerImage.FileName);
                    multipartContent.Add(new StringContent("banners"), "container");

                    var uploadResponse = await client.PostAsync("api/Files/upload", multipartContent);
                    if (uploadResponse.IsSuccessStatusCode)
                    {
                        var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(uploadJson);
                        if (doc.RootElement.TryGetProperty("url", out var urlProp))
                        {
                            bannerUrl = urlProp.GetString() ?? string.Empty;
                        }
                    }
                }
                catch
                {
                    // Fallback to empty if upload fails
                    bannerUrl = string.Empty;
                }
            }

            // 2. Persist Event via RaceDay.Api
            var payload = new
            {
                Name = name,
                Date = date == default ? DateTime.Now.AddDays(14) : date,
                Location = string.IsNullOrWhiteSpace(location) ? "Pretoria" : location,
                Distance = string.IsNullOrWhiteSpace(distance) ? "10km" : distance,
                EventType = string.IsNullOrWhiteSpace(eventType) ? "Road Running" : eventType,
                Description = string.IsNullOrWhiteSpace(description) ? "Standard Race Event" : description,
                BannerUrl = bannerUrl
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("api/Events", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Failed to create event ({(int)response.StatusCode}): {error}");
                return View();
            }

            return RedirectToAction("Events");
        }

        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            ViewBag.EventId = eventId;
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            try
            {
                var response = await client.GetAsync($"api/Events/{eventId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("name", out var nameProp))
                    {
                        ViewBag.EventName = nameProp.GetString();
                    }
                }
            }
            catch
            {
                ViewBag.EventName = $"Event #{eventId}";
            }

            try
            {
                var catResponse = await client.GetAsync($"api/Categories?eventId={eventId}");
                if (catResponse.IsSuccessStatusCode)
                {
                    ViewBag.CategoriesJson = await catResponse.Content.ReadAsStringAsync();
                }
                else
                {
                    ViewBag.CategoriesJson = "[]";
                }
            }
            catch
            {
                ViewBag.CategoriesJson = "[]";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, string categoryName, decimal entryFee, string distance)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            await AttachAuthHeaderAsync(client);

            var payload = new
            {
                EventId = eventId,
                Name = categoryName,
                Fee = entryFee,
                Distance = distance
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            await client.PostAsync("api/Categories", content);
            return RedirectToAction("Categories", new { eventId });
        }

        [HttpGet]
        public IActionResult Results(int eventId)
        {
            ViewBag.EventId = eventId;
            return View();
        }
    }
}