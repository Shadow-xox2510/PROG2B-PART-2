
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using RaceDayMVC.Models;

namespace RaceDayMVC.Controllers
{
    public class OrganiserController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public OrganiserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserID") == null ||
                HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public async Task<IActionResult> Enrolments(int eventId)
        {
            if (HttpContext.Session.GetInt32("UserID") == null ||
                HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Enrolments/event/{eventId}");

            ApiSessionHelper.AddApiSessionCookie(HttpContext, request);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["EventError"] =
                    "Could not load enrolments for this event.";

                return RedirectToAction("Index", "Events");
            }

            var enrolments = await response.Content
                .ReadFromJsonAsync<List<EnrolmentViewModel>>();

            ViewBag.EventID = eventId;

            return View(enrolments ?? new List<EnrolmentViewModel>());
        }


        [HttpGet]
        public IActionResult CaptureResult(int enrolmentId, int eventId, string participantName)
        {
            if (HttpContext.Session.GetInt32("UserID") == null ||
                HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new CaptureResultViewModel
            {
                EnrolmentID = enrolmentId,
                EventID = eventId,
                ParticipantName = participantName ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CaptureResult(CaptureResultViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserID") == null ||
                HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            if (!TimeSpan.TryParse(model.FinishTime, out TimeSpan finishTime) ||
                finishTime < TimeSpan.Zero)
            {
                ViewBag.ErrorMessage = "Please enter a valid finish time.";
                return View(model);
            }

            if (model.Position <= 0)
            {
                ViewBag.ErrorMessage = "Position must be greater than zero.";
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var resultData = new
            {
                EnrolmentID = model.EnrolmentID,
                FinishTime = finishTime,
                Position = model.Position,
                ResultStatus = model.ResultStatus
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post, "api/Results");

            ApiSessionHelper.AddApiSessionCookie(HttpContext, request);

            request.Content = System.Net.Http.Json.JsonContent.Create(resultData);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["EventSuccess"] = "Participant result recorded successfully.";

                return RedirectToAction("Enrolments", new { eventId = model.EventID });
            }

            var error = await response.Content.ReadAsStringAsync();

            ViewBag.ErrorMessage = "Could not record the result. " + error;

            return View(model);
        }

    }
}
