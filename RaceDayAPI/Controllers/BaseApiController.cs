using Microsoft.AspNetCore.Mvc;

namespace RaceDayAPI.Controllers
{
    public class BaseApiController : ControllerBase
    {
        protected int? GetUserId()
        {
            return HttpContext.Session.GetInt32("UserID");
        }

        protected string? GetUserRole()
        {
            return HttpContext.Session.GetString("Role");
        }

        protected IActionResult? RequireLogin()
        {
            if (!GetUserId().HasValue)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to access this endpoint."
                });
            }

            return null;
        }

        protected IActionResult? RequireRole(string requiredRole)
        {
            var loginCheck = RequireLogin();

            if (loginCheck != null)
            {
                return loginCheck;
            }

            if (GetUserRole() != requiredRole)
            {
                return StatusCode(403, new
                {
                    message = "You do not have permission to access this endpoint."
                });
            }

            return null;
        }

        protected IActionResult? RequireOwnership(int ownerId)
        {
            var loginCheck = RequireLogin();

            if (loginCheck != null)
            {
                return loginCheck;
            }

            if (GetUserId() != ownerId)
            {
                return StatusCode(403, new
                {
                    message = "You do not have permission to access this resource."
                });
            }

            return null;
        }
    }
}