using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEnrolment(EnrolmentDto dto)
        {
            var roleCheck = RequireRole("Participant");
            if (roleCheck != null) return roleCheck;

            var userId = GetUserId()!.Value;

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventID == dto.EventID);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryID == dto.CategoryID &&
                    c.EventID == dto.EventID);

            if (!categoryExists)
            {
                return BadRequest(new
                {
                    message = "The selected category does not belong to this event."
                });
            }

            var alreadyEnrolled = await _context.EventEnrolments
                .AnyAsync(e =>
                    e.EventID == dto.EventID &&
                    e.UserID == userId);

            if (alreadyEnrolled)
            {
                return BadRequest(new
                {
                    message = "You are already enrolled in this event."
                });
            }

            var enrolment = new EventEnrolment
            {
                EventID = dto.EventID,
                UserID = userId,
                CategoryID = dto.CategoryID,
                EnrolmentDate = DateTime.Now,
                Status = "Registered"
            };

            _context.EventEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment successful.",
                enrolmentID = enrolment.EnrolmentID
            });
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var roleCheck = RequireRole("Participant");
            if (roleCheck != null) return roleCheck;

            var userId = GetUserId()!.Value;

            var enrolments = await _context.EventEnrolments
                .Where(e => e.UserID == userId)
                .Join(
                    _context.Events,
                    e => e.EventID,
                    ev => ev.EventID,
                    (e, ev) => new
                    {
                        e.EnrolmentID,
                        e.EventID,
                        EventName = ev.EventName,
                        e.CategoryID,
                        e.EnrolmentDate,
                        e.Status
                    })
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var roleCheck = RequireRole("Organiser");
            if (roleCheck != null) return roleCheck;

            var userId = GetUserId()!.Value;

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventID == eventId);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var organiserOwnsEvent = await _context.Events
                .AnyAsync(e =>
                    e.EventID == eventId &&
                    e.OrganiserID == userId);

            if (!organiserOwnsEvent)
            {
                return StatusCode(403, new
                {
                    message = "You do not have permission to view enrolments for this event."
                });
            }

            var enrolments = await _context.EventEnrolments
                .Where(e => e.EventID == eventId)
                .Join(
                    _context.Users,
                    e => e.UserID,
                    u => u.UserID,
                    (e, u) => new
                    {
                        e.EnrolmentID,
                        e.EventID,
                        ParticipantID = u.UserID,
                        ParticipantName = u.FirstName + " " + u.LastName,
                        u.Email,
                        e.CategoryID,
                        e.EnrolmentDate,
                        e.Status
                    })
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}
