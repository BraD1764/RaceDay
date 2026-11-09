using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RaceDay.MVC.Models;
using System.Net.Http.Headers;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public ParticipantController(IHttpClientFactory clientFactory) { _clientFactory = clientFactory; }

        public async Task<IActionResult> MyEnrolments()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Account");

            var client = _clientFactory.CreateClient("RaceDayApi");
            var token = HttpContext.Session.GetString("JwtToken");

            // Add the token safely
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // this is for catching any errors so the app doesn't crash
            try
            {
                var response = await client.GetAsync("api/enrolments/my-enrolments");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var enrolments = JsonConvert.DeserializeObject<List<EnrolmentDto>>(json);
                    return View(enrolments);
                }
            }
            catch
            {
                // Ignore errors and show empty list
            }

            // If it fails or is empty, show an empty list instead of crashing
            return View(new List<EnrolmentDto>());
        }
    }
}