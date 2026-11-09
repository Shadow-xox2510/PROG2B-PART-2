using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using RaceDayMVC.Models;

namespace RaceDayMVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EventsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ==========================================
        // VIEW ALL EVENTS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var apiCookie = HttpContext.Session.GetString(
                    "ApiSessionCookie");

                Console.WriteLine(
                    $"Events: API session cookie present = {!string.IsNullOrWhiteSpace(apiCookie)}");

                if (string.IsNullOrWhiteSpace(apiCookie))
                {
                    ViewBag.ErrorMessage =
                        "Your API session is missing. Please log out and log in again.";

                    return View(new List<EventViewModel>());
                }

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/Events");

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    request);

                using var response = await client.SendAsync(request);

                Console.WriteLine(
                    $"Events API returned HTTP {(int)response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    Console.WriteLine(
                        $"EVENTS API ERROR: HTTP {(int)response.StatusCode}: {error}");

                    if (response.StatusCode ==
                        System.Net.HttpStatusCode.Unauthorized)
                    {
                        ViewBag.ErrorMessage =
                            "Your API session has expired. Please log out and log in again.";
                    }
                    else
                    {
                        ViewBag.ErrorMessage =
                            $"Events could not be loaded. API returned HTTP {(int)response.StatusCode}.";
                    }

                    return View(new List<EventViewModel>());
                }

                var events = await response.Content
                    .ReadFromJsonAsync<List<EventViewModel>>();

                return View(events ?? new List<EventViewModel>());
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"EVENTS CONNECTION ERROR: {ex.Message}");

                ViewBag.ErrorMessage =
                    "Unable to connect to the Events API.";

                return View(new List<EventViewModel>());
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine(
                    $"EVENTS JSON ERROR: {ex.Message}");

                ViewBag.ErrorMessage =
                    "The Events API returned data in an unexpected format.";

                return View(new List<EventViewModel>());
            }
        }

        // ==========================================
        // CREATE EVENT - GET
        // ==========================================

        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserID") == null ||
                HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new CreateEventViewModel());
        }

        // ==========================================
        // CREATE EVENT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserID") == null ||
                HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var eventData = new
                {
                    model.EventName,
                    model.Description,
                    model.EventDate,
                    model.Location,
                    DistanceKm = (decimal)model.DistanceKm,
                    model.EventType,
                    model.CategoryName,
                    model.CategoryDescription,
                    model.EntryFee,
                    model.MaximumParticipants
                };

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "api/Events");

                request.Content = JsonContent.Create(eventData);

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    request);

                Console.WriteLine(
                    "Sending Create Event request to RaceDay API...");

                using var response = await client.SendAsync(request);

                Console.WriteLine(
                    $"Create Event API returned HTTP {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["EventSuccess"] =
                        "Event created successfully!";

                    Console.WriteLine(
                        "Event created successfully.");

                    return RedirectToAction("Index", "Events");
                }

                var error = await response.Content.ReadAsStringAsync();

                Console.WriteLine(
                    $"CREATE EVENT ERROR: HTTP {(int)response.StatusCode}: {error}");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Your API session has expired. Please log out and log in again.");
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Event creation failed (HTTP {(int)response.StatusCode}). {error}");
                }

                return View(model);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"CREATE EVENT CONNECTION ERROR: {ex.Message}");

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the Events API.");

                return View(model);
            }
        }

        // ==========================================
        // EDIT EVENT - GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetInt32("UserID") == null ||
                HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/Events/{id}");

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    request);

                using var response = await client.SendAsync(request);

                Console.WriteLine(
                    $"Get Event API returned HTTP {(int)response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["EventError"] =
                        "Could not load the selected event.";

                    return RedirectToAction("Index");
                }

                var eventData = await response.Content
                    .ReadFromJsonAsync<EventViewModel>();

                if (eventData == null)
                {
                    TempData["EventError"] =
                        "The selected event could not be found.";

                    return RedirectToAction("Index");
                }

                // ======================================
                // LOAD EXISTING EVENT DETAILS
                // ======================================

                var model = new CreateEventViewModel
                {
                    EventName = eventData.EventName,

                    Description = eventData.Description ?? "",

                    EventDate = eventData.EventDate
                        ?? DateTime.Today.AddDays(1),

                    Location = eventData.Location ?? "",

                    EventType = eventData.EventType ?? "Run",

                    CategoryName = eventData.CategoryName,

                    CategoryDescription = eventData.CategoryDescription,

                    DistanceKm = eventData.DistanceKm,

                    EntryFee = eventData.EntryFee,

                    MaximumParticipants = eventData.MaximumParticipants
                };

                ViewBag.EventID = id;

                return View(model);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"GET EVENT CONNECTION ERROR: {ex.Message}");

                TempData["EventError"] =
                    "Unable to connect to the Events API.";

                return RedirectToAction("Index");
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine(
                    $"GET EVENT JSON ERROR: {ex.Message}");

                TempData["EventError"] =
                    "The Events API returned an unexpected response.";

                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // EDIT EVENT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CreateEventViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserID") == null ||
                HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.EventID = id;

                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                // ======================================
                // PREPARE UPDATED EVENT INFORMATION
                // ======================================

                var eventData = new
                {
                    model.EventName,
                    model.Description,
                    model.EventDate,
                    model.Location,
                    DistanceKm = (decimal)model.DistanceKm,
                    model.EventType,
                    model.CategoryName,
                    model.CategoryDescription,
                    model.EntryFee,
                    model.MaximumParticipants
                };

                using var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"api/Events/{id}");

                request.Content = JsonContent.Create(eventData);

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    request);

                Console.WriteLine(
                    $"Sending Edit Event request for Event ID {id}...");

                using var response = await client.SendAsync(request);

                Console.WriteLine(
                    $"Edit Event API returned HTTP {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["EventSuccess"] =
                        "Event updated successfully!";

                    return RedirectToAction("Index");
                }

                var error = await response.Content.ReadAsStringAsync();

                Console.WriteLine(
                    $"EDIT EVENT ERROR: HTTP {(int)response.StatusCode}: {error}");

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Your session has expired. Please log out and log in again.");
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Could not update the event. " + error);
                }

                ViewBag.EventID = id;

                return View(model);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"EDIT EVENT CONNECTION ERROR: {ex.Message}");

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the Events API.");

                ViewBag.EventID = id;

                return View(model);
            }
        }

        // ==========================================
        // DELETE EVENT - GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetInt32("UserID") == null ||
                HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Events/{id}");

            ApiSessionHelper.AddApiSessionCookie(
                HttpContext,
                request);

            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["EventError"] =
                    "Could not find the selected event.";

                return RedirectToAction("Index");
            }

            var eventData = await response.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (eventData == null)
            {
                TempData["EventError"] =
                    "The event could not be found.";

                return RedirectToAction("Index");
            }

            return View(eventData);
        }

        // ==========================================
        // DELETE EVENT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetInt32("UserID") == null ||
                HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/Events/{id}");

            ApiSessionHelper.AddApiSessionCookie(
                HttpContext,
                request);

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["EventSuccess"] =
                    "Event deleted successfully!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                Console.WriteLine(
                    $"DELETE EVENT ERROR: {error}");

                TempData["EventError"] =
                    "The event could not be deleted.";
            }

            return RedirectToAction("Index");
        }

        // ==========================================
        // UPLOAD EVENT BANNER
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadBanner(
            int id,
            IFormFile image)
        {
            if (HttpContext.Session.GetInt32("UserID") == null ||
                HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            if (image == null || image.Length == 0)
            {
                TempData["EventError"] =
                    "Please select an image.";

                return RedirectToAction("Edit", new { id });
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var form = new MultipartFormDataContent();

            using var fileStream = image.OpenReadStream();

            using var fileContent = new StreamContent(fileStream);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(image.ContentType);

            form.Add(
                fileContent,
                "image",
                image.FileName);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"api/Images/events/{id}/banner");

            ApiSessionHelper.AddApiSessionCookie(
                HttpContext,
                request);

            request.Content = form;

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["EventSuccess"] =
                    "Event banner uploaded successfully.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                Console.WriteLine(
                    $"BANNER UPLOAD ERROR: {error}");

                TempData["EventError"] =
                    "Could not upload event banner. " + error;
            }

            return RedirectToAction("Edit", new { id });
        }

        // ==========================================
        // DISPLAY EVENT BANNER
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Banner(int id)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return Unauthorized();
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/Images/events/{id}/banner");

            ApiSessionHelper.AddApiSessionCookie(
                HttpContext,
                request);

            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var imageBytes = await response.Content
                .ReadAsByteArrayAsync();

            var contentType =
                response.Content.Headers.ContentType?.MediaType
                ?? "image/jpeg";

            return File(
                imageBytes,
                contentType);
        }
    }
}