using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RaceDay.MVC.Models;
using System.Net.Http.Headers;
using System.Text;

namespace RaceDay.MVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IHttpClientFactory clientFactory, ILogger<EventsController> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var events = new List<EventDto>();

            try
            {
                var client = _clientFactory.CreateClient("RaceDayApi");
                var response = await client.GetAsync("api/events");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    events = JsonConvert.DeserializeObject<List<EventDto>>(json) ?? new List<EventDto>();
                }
                else
                {
                    _logger.LogWarning("The events API returned HTTP status {StatusCode}.", response.StatusCode);
                }
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "The events API could not be reached.");
            }
            catch (TaskCanceledException exception)
            {
                _logger.LogError(exception, "The events API request timed out.");
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "The events API returned invalid event data.");
            }

            return View(events);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Check if user is Organiser
            if (HttpContext.Session.GetString("UserRole") != "Organiser") return RedirectToAction("Login", "Account");

            return View(new EventDto
            {
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(1).AddHours(1),
                MaxParticipants = 1000
            });
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventDto model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser") return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var client = _clientFactory.CreateClient("RaceDayApi");
            var token = HttpContext.Session.GetString("JwtToken");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(
                JsonConvert.SerializeObject(model),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await client.PostAsync("api/events", content);

                if (response.IsSuccessStatusCode) return RedirectToAction("Dashboard", "Organiser");

                _logger.LogWarning("Creating an event failed with HTTP status {StatusCode}.", response.StatusCode);
                ViewBag.Error = "Failed to create event. Please check your inputs.";
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "The events API could not be reached while creating an event.");
                ViewBag.Error = "The events API could not be reached.";
            }
            catch (TaskCanceledException exception)
            {
                _logger.LogError(exception, "The event creation request timed out.");
                ViewBag.Error = "The event request timed out.";
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Enrol(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                TempData["ErrorMessage"] = "You must be logged in as a Participant to enrol.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Successfully enrolled! Check your 'My Enrolments' page.";
            return RedirectToAction(nameof(Index));
        }
    }
}
