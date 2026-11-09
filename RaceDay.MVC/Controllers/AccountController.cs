using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RaceDay.MVC.Models;
using System.Text;

namespace RaceDay.MVC.Controllers
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
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");

            // This ensures JSON is sent properly
            var jsonPayload = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("api/Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var auth = JsonConvert.DeserializeObject<AuthResponseDto>(jsonResponse);

                    // Save to Session
                    HttpContext.Session.SetString("JwtToken", auth.Token);
                    HttpContext.Session.SetString("UserRole", auth.Role);
                    HttpContext.Session.SetString("UserName", auth.FirstName);

                    // Redirect based on role
                    if (auth.Role == "Organiser") return RedirectToAction("Dashboard", "Organiser");
                    return RedirectToAction("Index", "Events");
                }

                // If API returns 400 or 401
                var errorContent = await response.Content.ReadAsStringAsync();
                ViewBag.Error = $"Login failed (Status {response.StatusCode}): {errorContent}";
                return View(model);
            }
            catch (Exception ex)
            {
                // If the API is completely unreachable or SSL fails
                ViewBag.Error = $"Connection Error: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Registration failed. Email might already exist.";
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}