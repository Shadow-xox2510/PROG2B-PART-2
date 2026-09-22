using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var loginCheck = RequireLogin();

            if (loginCheck != null)
            {
                return loginCheck;
            }

            var userId = GetUserId()!.Value;

            var user = await _context.Users
                .Where(u => u.UserID == userId)
                .Select(u => new
                {
                    u.UserID,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Role,
                    u.PhoneNumber,
                    u.DateRegistered
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User profile not found."
                });
            }

            return Ok(user);
        }

        // PUT: api/Profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(ProfileUpdateDto dto)
        {
            var loginCheck = RequireLogin();

            if (loginCheck != null)
            {
                return loginCheck;
            }

            var userId = GetUserId()!.Value;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User profile not found."
                });
            }

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.UserID != userId);

            if (emailExists)
            {
                return BadRequest(new
                {
                    message = "That email address is already being used."
                });
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully."
            });
        }
    }
}
