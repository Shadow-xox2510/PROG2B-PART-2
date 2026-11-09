
using Microsoft.AspNetCore.Http;

namespace RaceDayMVC.Models
{
    public static class ApiSessionHelper
    {
        public static void AddApiSessionCookie(
            HttpContext context,
            HttpRequestMessage request)
        {
            var cookie = context.Session.GetString("ApiSessionCookie");

            if (!string.IsNullOrWhiteSpace(cookie))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", cookie);
            }
        }
    }
}

