using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDayMVC.Models;

namespace RaceDayMVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProfileController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserID") != null;
        }

        // ==========================================
        // VIEW PROFILE - GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayAPI");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/Profile");

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    request);

                using var response = await client.SendAsync(request);

                Console.WriteLine(
                    $"PROFILE GET API returned HTTP {(int)response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode ==
                        HttpStatusCode.Unauthorized)
                    {
                        ViewBag.ErrorMessage =
                            "Your API session has expired. Please log out and log in again.";
                    }
                    else
                    {
                        ViewBag.ErrorMessage =
                            $"Could not load your profile. API returned HTTP {(int)response.StatusCode}.";
                    }

                    return View(new ProfileViewModel());
                }

                var profile = await response.Content
                    .ReadFromJsonAsync<ProfileViewModel>();

                if (profile == null)
                {
                    ViewBag.ErrorMessage =
                        "The API returned an empty profile.";

                    return View(new ProfileViewModel());
                }

                Console.WriteLine(
                    $"Profile loaded successfully for UserID {profile.UserID}");

                return View(profile);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"PROFILE CONNECTION ERROR: {ex.Message}");

                ViewBag.ErrorMessage =
                    "Unable to connect to the profile service.";

                return View(new ProfileViewModel());
            }
            catch (JsonException ex)
            {
                Console.WriteLine(
                    $"PROFILE JSON ERROR: {ex.Message}");

                ViewBag.ErrorMessage =
                    "The API returned profile information in an unexpected format.";

                return View(new ProfileViewModel());
            }
        }

        // ==========================================
        // UPDATE PROFILE - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            ProfileViewModel model)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayAPI");

                var profileData = new
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    Email = model.Email.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim()
                };

                using var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    "api/Profile");

                request.Content = JsonContent.Create(profileData);

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    request);

                using var response = await client.SendAsync(request);

                Console.WriteLine(
                    $"PROFILE UPDATE API returned HTTP {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["ProfileSuccess"] =
                        "Your profile has been updated successfully.";

                    return RedirectToAction(nameof(Index));
                }

                var error = await response.Content.ReadAsStringAsync();

                Console.WriteLine(
                    $"PROFILE UPDATE ERROR: HTTP {(int)response.StatusCode}: {error}");

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Your API session has expired. Please log out and log in again.");
                }
                else if (response.StatusCode ==
                    HttpStatusCode.BadRequest)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Profile update failed. Please check your details. The email address may already be in use.");
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Could not update your profile (HTTP {(int)response.StatusCode}).");
                }

                return View(model);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"PROFILE UPDATE CONNECTION ERROR: {ex.Message}");

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the profile service.");

                return View(model);
            }
        }

        // ==========================================
        // UPLOAD PROFILE PICTURE
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(
            IFormFile? image)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (image == null || image.Length == 0)
            {
                TempData["ProfileError"] =
                    "Please select an image.";

                return RedirectToAction(nameof(Index));
            }

            // Maximum file size: 5 MB
            const long maximumFileSize = 5 * 1024 * 1024;

            if (image.Length > maximumFileSize)
            {
                TempData["ProfileError"] =
                    "Profile pictures must be smaller than 5 MB.";

                return RedirectToAction(nameof(Index));
            }

            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension = Path.GetExtension(image.FileName);

            if (!allowedTypes.Contains(
                    image.ContentType,
                    StringComparer.OrdinalIgnoreCase) ||
                !allowedExtensions.Contains(
                    extension,
                    StringComparer.OrdinalIgnoreCase))
            {
                TempData["ProfileError"] =
                    "Please upload a JPG, PNG or WEBP image.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayAPI");

                using var form = new MultipartFormDataContent();

                using var fileStream = image.OpenReadStream();

                using var fileContent =
                    new StreamContent(fileStream);

                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(image.ContentType);

                form.Add(
                    fileContent,
                    "image",
                    Path.GetFileName(image.FileName));

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "api/Images/profile-picture");

                request.Content = form;

                ApiSessionHelper.AddApiSessionCookie(
                    HttpContext,
                    request);

                using var response = await client.SendAsync(request);

                Console.WriteLine(
                    $"PROFILE PICTURE UPLOAD API: HTTP {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["ProfileSuccess"] =
                        "Profile picture uploaded successfully.";
                }
                else
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    Console.WriteLine(
                        $"PROFILE PICTURE UPLOAD ERROR: {error}");

                    TempData["ProfileError"] =
                        $"Could not upload your picture (HTTP {(int)response.StatusCode}).";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"PROFILE PICTURE CONNECTION ERROR: {ex.Message}");

                TempData["ProfileError"] =
                    "Unable to connect to the picture upload service.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // DISPLAY PROFILE PICTURE
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> ProfilePicture()
        {
            if (!IsLoggedIn())
            {
                return Unauthorized();
            }

            try
            {
                var userId =
                    HttpContext.Session.GetInt32("UserID")!.Value;

                var client =
                    _httpClientFactory.CreateClient("RaceDayAPI");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/Images/profile-picture/{userId}");

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

                if (imageBytes.Length == 0)
                {
                    return NotFound();
                }

                var contentType =
                    response.Content.Headers.ContentType?.MediaType
                    ?? "image/jpeg";

                Response.Headers.CacheControl = "no-store";

                return File(imageBytes, contentType);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"PROFILE PICTURE ERROR: {ex.Message}");

                return StatusCode(502);
            }
        }
    }
}