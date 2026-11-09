
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Services;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : BaseApiController
    {
        private readonly RaceDayDbContext _context;
        private readonly BlobStorageService _blobStorage;

        public ImagesController(
            RaceDayDbContext context,
            BlobStorageService blobStorage)
        {
            _context = context;
            _blobStorage = blobStorage;
        }

        // POST: api/Images/profile-picture
        [HttpPost("profile-picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile image)
        {
            var loginCheck = RequireLogin();
            if (loginCheck != null) return loginCheck;

            var userId = GetUserId()!.Value;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            try
            {
                var oldImage = user.ProfilePictureName;

                var newImage = await _blobStorage.UploadImageAsync(
                    image, "ProfilePicturesContainer");

                user.ProfilePictureName = newImage;

                await _context.SaveChangesAsync();

                await _blobStorage.DeleteImageAsync(
                    oldImage, "ProfilePicturesContainer");

                return Ok(new
                {
                    message = "Profile picture uploaded successfully.",
                    fileName = newImage
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Images/profile-picture/5
        [HttpGet("profile-picture/{userId}")]
        public async Task<IActionResult> GetProfilePicture(int userId)
        {
            var loginCheck = RequireLogin();
            if (loginCheck != null) return loginCheck;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null || string.IsNullOrWhiteSpace(
                user.ProfilePictureName))
            {
                return NotFound(new
                {
                    message = "Profile picture not found."
                });
            }

            var stream = await _blobStorage.DownloadImageAsync(
                user.ProfilePictureName, "ProfilePicturesContainer");

            if (stream == null)
            {
                return NotFound(new
                {
                    message = "Profile picture not found in storage."
                });
            }

            return File(stream, GetImageContentType(
                user.ProfilePictureName));
        }

        // POST: api/Images/events/5/banner
        [HttpPost("events/{eventId}/banner")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadEventBanner(
            int eventId, IFormFile image)
        {
            var roleCheck = RequireRole("Organiser");
            if (roleCheck != null) return roleCheck;

            var eventDetails = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventDetails == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (eventDetails.OrganiserID != GetUserId()!.Value)
            {
                return StatusCode(403, new
                {
                    message = "You cannot upload a banner for this event."
                });
            }

            try
            {
                var oldImage = eventDetails.BannerImageName;

                var newImage = await _blobStorage.UploadImageAsync(
                    image, "EventBannersContainer");

                eventDetails.BannerImageName = newImage;

                await _context.SaveChangesAsync();

                await _blobStorage.DeleteImageAsync(
                    oldImage, "EventBannersContainer");

                return Ok(new
                {
                    message = "Event banner uploaded successfully.",
                    fileName = newImage
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Images/events/5/banner
        [HttpGet("events/{eventId}/banner")]
        public async Task<IActionResult> GetEventBanner(int eventId)
        {
            var loginCheck = RequireLogin();
            if (loginCheck != null) return loginCheck;

            var eventDetails = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventDetails == null || string.IsNullOrWhiteSpace(
                eventDetails.BannerImageName))
            {
                return NotFound(new
                {
                    message = "Event banner not found."
                });
            }

            var stream = await _blobStorage.DownloadImageAsync(
                eventDetails.BannerImageName, "EventBannersContainer");

            if (stream == null)
            {
                return NotFound(new
                {
                    message = "Event banner not found in storage."
                });
            }

            return File(stream, GetImageContentType(
                eventDetails.BannerImageName));
        }

        private string GetImageContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (extension == ".png") return "image/png";
            if (extension == ".webp") return "image/webp";

            return "image/jpeg";
        }
    }
}


