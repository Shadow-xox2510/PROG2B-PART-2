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

        // ==========================================
        // GET USER PROFILE
        // GET: api/Profile
        // ==========================================

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
                .AsNoTracking()
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

        // ==========================================
        // UPDATE USER PROFILE
        // PUT: api/Profile
        // ==========================================

        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            ProfileUpdateDto dto)
        {
            var loginCheck = RequireLogin();

            if (loginCheck != null)
            {
                return loginCheck;
            }

            if (dto == null)
            {
                return BadRequest(new
                {
                    message = "Profile information is required."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.FirstName))
            {
                return BadRequest(new
                {
                    message = "First name is required."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.LastName))
            {
                return BadRequest(new
                {
                    message = "Last name is required."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new
                {
                    message = "Email address is required."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                return BadRequest(new
                {
                    message = "Phone number is required."
                });
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

            var email = dto.Email.Trim();

            var emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Email == email &&
                    u.UserID != userId);

            if (emailExists)
            {
                return BadRequest(new
                {
                    message = "That email address is already being used."
                });
            }

            user.FirstName = dto.FirstName.Trim();

            user.LastName = dto.LastName.Trim();

            user.Email = email;

            user.PhoneNumber = dto.PhoneNumber.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully."
            });
        }
    }
}