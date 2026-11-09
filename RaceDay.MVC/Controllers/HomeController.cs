using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using RaceDay.MVC.Models;
using System.Diagnostics;

namespace RaceDay.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var events = new List<EventDto>();

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayApi");
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
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayApi");
                var response = await client.GetAsync($"api/events/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return NotFound();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode);

                var json = await response.Content.ReadAsStringAsync();
                var eventDto = JsonConvert.DeserializeObject<EventDto>(json);

                return eventDto is null ? NotFound() : View(eventDto);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "The events API could not be reached while loading event {EventId}.", id);
                return NotFound();
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "The events API returned invalid data for event {EventId}.", id);
                return NotFound();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // This is a custom attribute to check Session before allowing access
        public class SessionAuthorizeAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext context)
            {
                var token = context.HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                }
            }
        }
    }
}
