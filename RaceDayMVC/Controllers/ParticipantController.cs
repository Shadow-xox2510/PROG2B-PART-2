using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDayMVC.Models;

namespace RaceDayMVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsParticipant()
        {
            return HttpContext.Session.GetInt32("UserID") != null
                && HttpContext.Session.GetString("Role") == "Participant";
        }

        // ==========================================
        // PARTICIPANT DASHBOARD
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Account");
            }

            var events = new List<EventViewModel>();

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/Events");

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    request);

                using var response = await client.SendAsync(request);

                Console.WriteLine(
                    $"Participant Dashboard API: HTTP {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    events = await response.Content
                        .ReadFromJsonAsync<List<EventViewModel>>()
                        ?? new List<EventViewModel>();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ViewBag.ErrorMessage =
                        "Your API session has expired. Please log out and log in again.";
                }
                else
                {
                    ViewBag.ErrorMessage =
                        $"Events could not be loaded. API returned HTTP {(int)response.StatusCode}.";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"PARTICIPANT DASHBOARD CONNECTION ERROR: {ex.Message}");

                ViewBag.ErrorMessage =
                    "Unable to connect to the RaceDay API.";
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"PARTICIPANT DASHBOARD JSON ERROR: {ex.Message}");

                ViewBag.ErrorMessage =
                    "The API returned event information in an unexpected format.";
            }

            return View(events);
        }

        // ==========================================
        // EVENT DETAILS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> EventDetails(int id)
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id <= 0)
            {
                TempData["EventError"] = "Invalid event selected.";

                return RedirectToAction("Index", "Events");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                // Get event information
                using var eventRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/Events/{id}");

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    eventRequest);

                using var eventResponse =
                    await client.SendAsync(eventRequest);

                Console.WriteLine(
                    $"Participant Event Details API: HTTP {(int)eventResponse.StatusCode}");

                if (!eventResponse.IsSuccessStatusCode)
                {
                    TempData["EventError"] =
                        "Could not load the selected event.";

                    return RedirectToAction("Index", "Events");
                }

                var eventData = await eventResponse.Content
                    .ReadFromJsonAsync<EventViewModel>();

                if (eventData == null)
                {
                    TempData["EventError"] =
                        "The selected event was not found.";

                    return RedirectToAction("Index", "Events");
                }

                var model = new ParticipantEventViewModel
                {
                    Event = eventData,
                    Categories = new List<CategoryViewModel>()
                };

                // Get categories for the selected event
                using var categoryRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/Categories/event/{id}");

                // IMPORTANT: Forward API login session
                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    categoryRequest);

                using var categoryResponse =
                    await client.SendAsync(categoryRequest);

                Console.WriteLine(
                    $"Participant Categories API: HTTP {(int)categoryResponse.StatusCode}");

                if (categoryResponse.IsSuccessStatusCode)
                {
                    model.Categories = await categoryResponse.Content
                        .ReadFromJsonAsync<List<CategoryViewModel>>()
                        ?? new List<CategoryViewModel>();
                }
                else
                {
                    var error = await categoryResponse.Content
                        .ReadAsStringAsync();

                    Console.WriteLine(
                        $"CATEGORIES API ERROR: {error}");

                    ViewBag.CategoryError =
                        $"Categories could not be loaded (HTTP {(int)categoryResponse.StatusCode}).";
                }

                return View(model);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"EVENT DETAILS CONNECTION ERROR: {ex.Message}");

                TempData["EventError"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction("Index", "Events");
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"EVENT DETAILS JSON ERROR: {ex.Message}");

                TempData["EventError"] =
                    "The API returned invalid event information.";

                return RedirectToAction("Index", "Events");
            }
        }

        // ==========================================
        // ENROL IN EVENT
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(
            int eventId,
            int categoryId)
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Account");
            }

            if (eventId <= 0 || categoryId <= 0)
            {
                TempData["EnrolmentError"] =
                    "Please select a valid event category.";

                return RedirectToAction(
                    "EventDetails",
                    new { id = eventId });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var enrolmentData = new
                {
                    EventID = eventId,
                    CategoryID = categoryId
                };

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "api/Enrolments");

                request.Content = JsonContent.Create(enrolmentData);

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    request);

                using var response = await client.SendAsync(request);

                Console.WriteLine(
                    $"Enrolment API returned HTTP {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["EnrolmentSuccess"] =
                        "You have successfully enrolled in the event!";

                    return RedirectToAction("MyEvents");
                }

                var error = await response.Content.ReadAsStringAsync();

                Console.WriteLine(
                    $"ENROLMENT ERROR: HTTP {(int)response.StatusCode}: {error}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TempData["EnrolmentError"] =
                        "Your session has expired. Please log out and log in again.";
                }
                else
                {
                    TempData["EnrolmentError"] =
                        "Enrolment could not be completed. You may already be registered, or the selected category may not be available.";
                }

                return RedirectToAction(
                    "EventDetails",
                    new { id = eventId });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"ENROLMENT CONNECTION ERROR: {ex.Message}");

                TempData["EnrolmentError"] =
                    "Unable to connect to the enrolment service.";

                return RedirectToAction(
                    "EventDetails",
                    new { id = eventId });
            }
        }

        // ==========================================
        // MY ENROLMENTS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Account");
            }

            var enrolments = new List<EnrolmentViewModel>();

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/Enrolments/my");

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    request);

                using var response = await client.SendAsync(request);

                Console.WriteLine(
                    $"My Enrolments API: HTTP {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    enrolments = await response.Content
                        .ReadFromJsonAsync<List<EnrolmentViewModel>>()
                        ?? new List<EnrolmentViewModel>();
                }
                else
                {
                    ViewBag.ErrorMessage =
                        $"Could not load your enrolments (HTTP {(int)response.StatusCode}).";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"MY ENROLMENTS CONNECTION ERROR: {ex.Message}");

                ViewBag.ErrorMessage =
                    "Unable to connect to the enrolment service.";
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"MY ENROLMENTS JSON ERROR: {ex.Message}");

                ViewBag.ErrorMessage =
                    "The API returned enrolment data in an unexpected format.";
            }

            return View(enrolments);
        }

        // ==========================================
        // MY RACE RESULTS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Account");
            }

            var results = new List<ResultViewModel>();

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/Results/my");

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    request);

                using var response = await client.SendAsync(request);

                Console.WriteLine(
                    $"My Results API: HTTP {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    results = await response.Content
                        .ReadFromJsonAsync<List<ResultViewModel>>()
                        ?? new List<ResultViewModel>();
                }
                else
                {
                    ViewBag.ErrorMessage =
                        $"Could not load your race results (HTTP {(int)response.StatusCode}).";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"MY RESULTS CONNECTION ERROR: {ex.Message}");

                ViewBag.ErrorMessage =
                    "Unable to connect to the results service.";
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"MY RESULTS JSON ERROR: {ex.Message}");

                ViewBag.ErrorMessage =
                    "The API returned results in an unexpected format.";
            }

            return View(results);
        }
    }
}