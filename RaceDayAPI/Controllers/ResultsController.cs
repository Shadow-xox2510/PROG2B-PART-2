using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Organisers capture a result for a participant
        [HttpPost]
        public async Task<IActionResult> CreateResult(ResultDto dto)
        {
            var roleCheck = RequireRole("Organiser");
            if (roleCheck != null) return roleCheck;

            var userId = GetUserId()!.Value;

            var enrolment = await _context.EventEnrolments
                .Join(
                    _context.Events,
                    e => e.EventID,
                    ev => ev.EventID,
                    (e, ev) => new { Enrolment = e, Event = ev }
                )
                .FirstOrDefaultAsync(x =>
                    x.Enrolment.EnrolmentID == dto.EnrolmentID &&
                    x.Event.OrganiserID == userId);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found or you do not own this event."
                });
            }

            var resultExists = await _context.Results
                .AnyAsync(r => r.EnrolmentID == dto.EnrolmentID);

            if (resultExists)
            {
                return BadRequest(new
                {
                    message = "A result already exists for this enrolment."
                });
            }

            if (dto.FinishTime < TimeSpan.Zero)
            {
                return BadRequest(new
                {
                    message = "Finish time cannot be negative."
                });
            }

            if (dto.Position <= 0)
            {
                return BadRequest(new
                {
                    message = "Position must be greater than zero."
                });
            }

            var result = new Result
            {
                EnrolmentID = dto.EnrolmentID,
                FinishTime = dto.FinishTime,
                Position = dto.Position,
                ResultStatus = dto.ResultStatus
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result recorded successfully.",
                resultID = result.ResultID
            });
        }

        // Participants view their own results
        [HttpGet("my")]
        public async Task<IActionResult> GetMyResults()
        {
            var roleCheck = RequireRole("Participant");
            if (roleCheck != null) return roleCheck;

            var userId = GetUserId()!.Value;

            var results = await _context.Results
                .Join(
                    _context.EventEnrolments,
                    r => r.EnrolmentID,
                    e => e.EnrolmentID,
                    (r, e) => new { Result = r, Enrolment = e }
                )
                .Join(
                    _context.Events,
                    x => x.Enrolment.EventID,
                    ev => ev.EventID,
                    (x, ev) => new
                    {
                        x.Result.ResultID,
                        x.Result.EnrolmentID,
                        EventID = ev.EventID,
                        EventName = ev.EventName,
                        FinishTime = x.Result.FinishTime,
                        Position = x.Result.Position,
                        ResultStatus = x.Result.ResultStatus
                    }
                )
                .Join(
                    _context.EventEnrolments,
                    r => r.EnrolmentID,
                    e => e.EnrolmentID,
                    (r, e) => new { Result = r, Enrolment = e }
                )
                .Where(x => x.Enrolment.UserID == userId)
                .Select(x => x.Result)
                .ToListAsync();

            return Ok(results);
        }
    }
}
