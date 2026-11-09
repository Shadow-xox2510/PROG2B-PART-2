using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using RaceDayMVC.Models;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ==========================================
        // REGISTER - GET
        // ==========================================

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // ==========================================
        // REGISTER - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine(
                            $"REGISTRATION VALIDATION ERROR - {entry.Key}: {error.ErrorMessage}");
                    }
                }

                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                Console.WriteLine(
                    "Sending registration request to RaceDay API...");

                var response = await client.PostAsJsonAsync(
                    "api/Auth/register",
                    model);

                Console.WriteLine(
                    $"Registration API returned HTTP {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["RegistrationSuccess"] =
                        "Registration successful! You can now log in.";

                    Console.WriteLine(
                        "Registration completed successfully.");

                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Registration failed. Please check your details. " +
                    "The email address may already be registered.");

                return View(model);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"REGISTRATION CONNECTION ERROR: {ex.Message}");

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the registration service.");

                return View(model);
            }
        }

        // ==========================================
        // LOGIN - GET
        // ==========================================

        [HttpGet]
        public IActionResult Login()
        {
            // If the user is already logged in,
            // redirect them to their dashboard.

            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId.HasValue)
            {
                if (role == "Organiser")
                {
                    return RedirectToAction("Index", "Organiser");
                }

                if (role == "Participant")
                {
                    return RedirectToAction("Index", "Participant");
                }
            }

            return View(new LoginViewModel());
        }

        // ==========================================
        // LOGIN - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine(
                            $"LOGIN VALIDATION ERROR - {entry.Key}: {error.ErrorMessage}");
                    }
                }

                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                Console.WriteLine(
                    "Sending login request to RaceDay API...");

                var response = await client.PostAsJsonAsync(
                    "api/Auth/login",
                    model);

                Console.WriteLine(
                    $"Login API returned HTTP {(int)response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Login failed. Please check your email and password.");

                    return View(model);
                }

                // ==========================================
                // READ API LOGIN RESPONSE
                // ==========================================

                var loginResult = await response.Content
                    .ReadFromJsonAsync<LoginResponse>();

                if (loginResult == null ||
                    loginResult.userID <= 0 ||
                    (loginResult.role != "Organiser" &&
                     loginResult.role != "Participant"))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The API returned an unexpected login response.");

                    return View(model);
                }

                // ==========================================
                // START A CLEAN MVC SESSION
                // ==========================================

                HttpContext.Session.Clear();

                // Preserve the API session cookie if supplied.

                if (response.Headers.TryGetValues(
                    "Set-Cookie",
                    out var cookieHeaders))
                {
                    var apiCookie = cookieHeaders.FirstOrDefault(
                        c => c.StartsWith(
                            ".AspNetCore.Session=",
                            StringComparison.OrdinalIgnoreCase));

                    if (apiCookie != null)
                    {
                        HttpContext.Session.SetString(
                            "ApiSessionCookie",
                            apiCookie.Split(';')[0]);
                    }
                }

                // ==========================================
                // SAVE USER LOGIN SESSION
                // ==========================================

                HttpContext.Session.SetInt32(
                    "UserID",
                    loginResult.userID);

                HttpContext.Session.SetString(
                    "Role",
                    loginResult.role);

                TempData["LoginSuccess"] =
                    "You have logged in successfully!";

                Console.WriteLine(
                    $"Login completed successfully. Role: {loginResult.role}");

                // ==========================================
                // REDIRECT BASED ON USER ROLE
                // ==========================================

                if (loginResult.role == "Organiser")
                {
                    return RedirectToAction(
                        "Index",
                        "Organiser");
                }

                return RedirectToAction(
                    "Index",
                    "Participant");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"LOGIN CONNECTION ERROR: {ex.Message}");

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the login service.");

                return View(model);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine(
                    $"LOGIN RESPONSE ERROR: {ex.Message}");

                ModelState.AddModelError(
                    string.Empty,
                    "The login service returned an invalid response.");

                return View(model);
            }
        }

        // ==========================================
        // LOGOUT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Remove user information from MVC session.
            HttpContext.Session.Clear();

            TempData["LogoutSuccess"] =
                "You have logged out successfully.";

            Console.WriteLine(
                "User logged out successfully.");

            return RedirectToAction(
                "Login",
                "Account");
        }
    }

    // ==========================================
    // LOGIN API RESPONSE MODEL
    // ==========================================

    public class LoginResponse
    {
        public string message { get; set; } = "";

        public int userID { get; set; }

        public string role { get; set; } = "";
    }
}