
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var loginCheck = RequireLogin();
            if (loginCheck != null) return loginCheck;

            var events = await _context.Events
                .Join(
                    _context.Routes,
                    e => e.RouteID,
                    r => r.RouteID,
                    (e, r) => new
                    {
                        e.EventID,
                        e.EventName,
                        e.Description,
                        e.EventDate,
                        e.Location,
                        DistanceKm = r.DistanceKm,
                        e.EventType,
                        e.OrganiserID,
                        e.Status,
                        e.EntryFee,
                        e.MaximumParticipants
                    })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/5
        // GET: api/Events/5

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var loginCheck = RequireLogin();

            if (loginCheck != null)
            {
                return loginCheck;
            }

            var eventDetails = await _context.Events
                .Where(e => e.EventID == id)
                .Join(
                    _context.Routes,
                    e => e.RouteID,
                    r => r.RouteID,
                    (e, r) => new
                    {
                        e.EventID,
                        e.EventName,
                        e.Description,
                        e.EventDate,
                        e.Location,
                        DistanceKm = r.DistanceKm,
                        e.EventType,
                        e.OrganiserID,
                        e.Status,
                        e.EntryFee,
                        e.MaximumParticipants
                    })
                .FirstOrDefaultAsync();

            if (eventDetails == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Retrieve the category associated with this event.

            var category = await _context.Categories
                .Where(c => c.EventID == id)
                .OrderBy(c => c.CategoryID)
                .Select(c => new
                {
                    c.CategoryName,
                    c.Description
                })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                eventDetails.EventID,
                eventDetails.EventName,
                eventDetails.Description,
                eventDetails.EventDate,
                eventDetails.Location,
                eventDetails.DistanceKm,
                eventDetails.EventType,
                eventDetails.OrganiserID,
                eventDetails.Status,
                eventDetails.EntryFee,
                eventDetails.MaximumParticipants,

                CategoryName = category?.CategoryName ?? "",
                CategoryDescription = category?.Description ?? ""
            });
        }


        // POST: api/Events
        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventDto dto)
        {
            var roleCheck = RequireRole("Organiser");
            if (roleCheck != null) return roleCheck;

            if (dto.EventType != "Run" &&
                dto.EventType != "Walk" &&
                dto.EventType != "Cycle")
            {
                return BadRequest(new
                {
                    message = "Event type must be Run, Walk or Cycle."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.EventName))
            {
                return BadRequest(new
                {
                    message = "Event name is required."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.CategoryName))
            {
                return BadRequest(new
                {
                    message = "Category name is required."
                });
            }

            if (dto.DistanceKm <= 0 ||
                dto.MaximumParticipants <= 0)
            {
                return BadRequest(new
                {
                    message = "Distance and maximum participants must be greater than zero."
                });
            }

            var route = new RaceDayAPI.Models.Route
            {
                RouteName = dto.EventName + " Route",
                DistanceKm = dto.DistanceKm,
                StartLocation = dto.Location,
                FinishLocation = dto.Location
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            var newEvent = new Event
            {
                OrganiserID = GetUserId()!.Value,
                RouteID = route.RouteID,
                EventName = dto.EventName,
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location,
                EntryFee = dto.EntryFee,
                MaximumParticipants = dto.MaximumParticipants,
                Status = "Active",
                EventType = dto.EventType
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            var category = new Category
            {
                EventID = newEvent.EventID,
                CategoryName = dto.CategoryName,
                Description = dto.CategoryDescription
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event created successfully.",
                eventID = newEvent.EventID,
                categoryID = category.CategoryID
            });
        }



        // PUT: api/Events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id, EventDto dto)
        {
            var roleCheck = RequireRole("Organiser");
            if (roleCheck != null) return roleCheck;

            if (dto.EventType != "Run" &&
                dto.EventType != "Walk" &&
                dto.EventType != "Cycle")
            {
                return BadRequest(new
                {
                    message = "Event type must be Run, Walk or Cycle."
                });
            }

            var eventToUpdate = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventToUpdate == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (eventToUpdate.OrganiserID != GetUserId()!.Value)
            {
                return StatusCode(403, new
                {
                    message = "You do not have permission to modify this event."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.CategoryName))
            {
                return BadRequest(new
                {
                    message = "Category name is required."
                });
            }

            if (dto.DistanceKm <= 0 ||
                dto.MaximumParticipants <= 0)
            {
                return BadRequest(new
                {
                    message = "Distance and maximum participants must be greater than zero."
                });
            }

            var route = await _context.Routes
                .FirstOrDefaultAsync(
                    r => r.RouteID == eventToUpdate.RouteID);

            if (route == null)
            {
                return NotFound(new
                {
                    message = "Event route not found."
                });
            }

            route.DistanceKm = dto.DistanceKm;
            route.StartLocation = dto.Location;
            route.FinishLocation = dto.Location;

            eventToUpdate.EventName = dto.EventName;
            eventToUpdate.Description = dto.Description;
            eventToUpdate.EventDate = dto.EventDate;
            eventToUpdate.Location = dto.Location;
            eventToUpdate.EntryFee = dto.EntryFee;
            eventToUpdate.MaximumParticipants =
                dto.MaximumParticipants;
            eventToUpdate.EventType = dto.EventType;

            var category = await _context.Categories
                .FirstOrDefaultAsync(
                    c => c.EventID == eventToUpdate.EventID);

            if (category != null)
            {
                category.CategoryName = dto.CategoryName;
                category.Description = dto.CategoryDescription;
            }
            else
            {
                category = new Category
                {
                    EventID = eventToUpdate.EventID,
                    CategoryName = dto.CategoryName,
                    Description = dto.CategoryDescription
                };

                _context.Categories.Add(category);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully."
            });
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var roleCheck = RequireRole("Organiser");
            if (roleCheck != null) return roleCheck;

            var eventToDelete = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventToDelete == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (eventToDelete.OrganiserID != GetUserId()!.Value)
            {
                return StatusCode(403, new
                {
                    message = "You do not have permission to delete this event."
                });
            }

            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }
    }
}
