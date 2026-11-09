using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var payload = new { Email = email.Trim(), Password = password.Trim() };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("api/Auth/login", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var errDoc = JsonDocument.Parse(responseBody);
                        if (errDoc.RootElement.TryGetProperty("message", out var msgProp))
                        {
                            ViewBag.Error = msgProp.GetString();
                        }
                        else
                        {
                            ViewBag.Error = "Invalid email or password.";
                        }
                    }
                    catch
                    {
                        ViewBag.Error = "Invalid email or password.";
                    }
                    return View();
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                string token = root.TryGetProperty("token", out var tProp) ? tProp.GetString() ?? "" : "";
                string role = root.TryGetProperty("role", out var rProp) ? rProp.GetString() ?? "Participant" : "Participant";
                string name = root.TryGetProperty("name", out var nProp) ? nProp.GetString() ?? email : email;

                HttpContext.Session.SetString("JWToken", token);
                HttpContext.Session.SetString("UserRole", role);
                HttpContext.Session.SetString("UserName", name);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                if (role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Organiser");
                }

                return RedirectToAction("Events", "Participant");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Cannot connect to backend API: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var payload = new
            {
                FullName = fullName?.Trim() ?? "RaceDay User",
                Email = email.Trim(),
                Password = password.Trim(),
                Role = string.IsNullOrWhiteSpace(role) ? "Participant" : role.Trim()
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("api/Auth/register", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var errDoc = JsonDocument.Parse(responseBody);
                        if (errDoc.RootElement.TryGetProperty("message", out var msgProp))
                        {
                            ViewBag.Error = msgProp.GetString();
                        }
                        else
                        {
                            ViewBag.Error = "Registration failed.";
                        }
                    }
                    catch
                    {
                        ViewBag.Error = "Registration failed. Please try again.";
                    }
                    return View();
                }

                TempData["Success"] = "Account created successfully! Please log in.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"API connection error: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}