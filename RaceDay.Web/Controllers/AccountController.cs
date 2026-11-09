using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
                return View(model);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var user = JsonSerializer.Deserialize<UserDto>(responseBody, options);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user?.Name ?? model.Email),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Role, user?.Role ?? "Participant"),
                new Claim("UserId", user?.UserId.ToString() ?? "0")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            if (user?.Role == "Organiser")
            {
                return RedirectToAction("Dashboard", "Organiser");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Registration failed. Email might already exist.");
                return View(model);
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}