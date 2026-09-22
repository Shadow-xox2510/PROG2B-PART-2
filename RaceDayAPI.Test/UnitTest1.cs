using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Controllers;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Tests
{
    public class UnitTest1
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private DefaultHttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Session = new TestSession();
            return context;
        }

        private User CreateUser(
            RaceDayDbContext context,
            string email,
            string role,
            string password)
        {
            var user = new User
            {
                FirstName = "Test",
                LastName = role,
                Email = email,
                Role = role,
                PhoneNumber = "0821234567",
                DateRegistered = DateTime.Now
            };

            var passwordHasher = new PasswordHasher<User>();

            user.PasswordHash =
                passwordHasher.HashPassword(user, password);

            context.Users.Add(user);
            context.SaveChanges();

            return user;
        }

        // TEST 1
        [Fact]
        public async Task Register_ShouldCreateUser()
        {
            using var context = CreateContext();

            var controller = new AuthController(context);

            var dto = new RegisterDto
            {
                FirstName = "John",
                LastName = "Organiser",
                Email = "john@test.com",
                Password = "Password123!",
                Role = "Organiser",
                PhoneNumber = "0821234567"
            };

            var result = await controller.Register(dto);

            Assert.IsType<OkObjectResult>(result);

            var user = await context.Users.FirstAsync();

            Assert.Equal("john@test.com", user.Email);
            Assert.Equal("Organiser", user.Role);

            // Confirms password is hashed rather than stored as plain text.
            Assert.NotEqual("Password123!", user.PasswordHash);
        }

        // TEST 2
        [Fact]
        public async Task Login_ShouldStoreUserIdAndRoleInSession()
        {
            using var context = CreateContext();

            var user = CreateUser(
                context,
                "login@test.com",
                "Participant",
                "Password123!");

            var controller = new AuthController(context);

            var httpContext = CreateHttpContext();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var dto = new LoginDto
            {
                Email = "login@test.com",
                Password = "Password123!"
            };

            var result = await controller.Login(dto);

            Assert.IsType<OkObjectResult>(result);

            Assert.Equal(
                user.UserID,
                httpContext.Session.GetInt32("UserID"));

            Assert.Equal(
                "Participant",
                httpContext.Session.GetString("Role"));
        }

        // TEST 3
        [Fact]
        public async Task Profile_ShouldRejectUnauthenticatedUser()
        {
            using var context = CreateContext();

            var controller = new ProfileController(context);

            var httpContext = CreateHttpContext();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var result = await controller.GetProfile();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // TEST 4
        [Fact]
        public async Task Organiser_ShouldBeAbleToCreateEvent()
        {
            using var context = CreateContext();

            var organiser = CreateUser(
                context,
                "organiser@test.com",
                "Organiser",
                "Password123!");

            var controller = new EventsController(context);

            var httpContext = CreateHttpContext();

            httpContext.Session.SetInt32("UserID", organiser.UserID);
            httpContext.Session.SetString("Role", "Organiser");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var dto = new EventDto
            {
                EventName = "Test Fun Run",
                Description = "Testing event creation.",
                EventDate = DateTime.Now.AddDays(30),
                Location = "Cape Town",
                DistanceKm = 10,
                EventType = "Run",
                CategoryName = "10KM Run",
                CategoryDescription = "Test category",
                EntryFee = 100,
                MaximumParticipants = 50
            };

            var result = await controller.CreateEvent(dto);

            Assert.IsType<OkObjectResult>(result);

            Assert.Equal(1, await context.Events.CountAsync());
            Assert.Equal(1, await context.Categories.CountAsync());
            Assert.Equal(1, await context.Routes.CountAsync());

            var createdEvent = await context.Events.FirstAsync();

            Assert.Equal("Test Fun Run", createdEvent.EventName);
            Assert.Equal(organiser.UserID, createdEvent.OrganiserID);
        }

        // TEST 5
        [Fact]
        public async Task Participant_ShouldBeRejectedFromCreatingEvent()
        {
            using var context = CreateContext();

            var participant = CreateUser(
                context,
                "participant@test.com",
                "Participant",
                "Password123!");

            var controller = new EventsController(context);

            var httpContext = CreateHttpContext();

            httpContext.Session.SetInt32("UserID", participant.UserID);
            httpContext.Session.SetString("Role", "Participant");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var dto = new EventDto
            {
                EventName = "Unauthorized Event",
                Description = "This should not be created.",
                EventDate = DateTime.Now.AddDays(30),
                Location = "Cape Town",
                DistanceKm = 10,
                EventType = "Run",
                CategoryName = "10KM Run",
                CategoryDescription = "Test category",
                EntryFee = 100,
                MaximumParticipants = 50
            };

            var result = await controller.CreateEvent(dto);

            var statusResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(403, statusResult.StatusCode);

            Assert.Empty(context.Events);
        }

        // TEST 6
        [Fact]
        public async Task Participant_ShouldBeAbleToEnrolInEvent()
        {
            using var context = CreateContext();

            var organiser = CreateUser(
                context,
                "organiser@test.com",
                "Organiser",
                "Password123!");

            var participant = CreateUser(
                context,
                "participant@test.com",
                "Participant",
                "Password123!");

            var route = new RaceDayAPI.Models.Route
            {
                RouteName = "Test Route",
                DistanceKm = 10,
                StartLocation = "Cape Town",
                FinishLocation = "Cape Town"
            };

            context.Routes.Add(route);
            await context.SaveChangesAsync();

            var testEvent = new Event
            {
                OrganiserID = organiser.UserID,
                RouteID = route.RouteID,
                EventName = "Test Event",
                Description = "Test event",
                EventDate = DateTime.Now.AddDays(30),
                Location = "Cape Town",
                EntryFee = 100,
                MaximumParticipants = 50,
                Status = "Active",
                EventType = "Run"
            };

            context.Events.Add(testEvent);
            await context.SaveChangesAsync();

            var category = new Category
            {
                EventID = testEvent.EventID,
                CategoryName = "10KM Run",
                Description = "Test category"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var controller = new EnrolmentsController(context);

            var httpContext = CreateHttpContext();

            httpContext.Session.SetInt32("UserID", participant.UserID);
            httpContext.Session.SetString("Role", "Participant");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var dto = new EnrolmentDto
            {
                EventID = testEvent.EventID,
                CategoryID = category.CategoryID
            };

            var result = await controller.CreateEnrolment(dto);

            Assert.IsType<OkObjectResult>(result);

            var enrolment =
                await context.EventEnrolments.FirstAsync();

            Assert.Equal(
                participant.UserID,
                enrolment.UserID);

            Assert.Equal(
                testEvent.EventID,
                enrolment.EventID);

            Assert.Equal(
                category.CategoryID,
                enrolment.CategoryID);

            Assert.Equal(
                "Registered",
                enrolment.Status);
        }

        // TEST 7
        [Fact]
        public async Task Organiser_ShouldBeAbleToRecordResult()
        {
            using var context = CreateContext();

            var organiser = CreateUser(
                context,
                "organiser@test.com",
                "Organiser",
                "Password123!");

            var participant = CreateUser(
                context,
                "participant@test.com",
                "Participant",
                "Password123!");

            var route = new RaceDayAPI.Models.Route
            {
                RouteName = "Test Route",
                DistanceKm = 10,
                StartLocation = "Cape Town",
                FinishLocation = "Cape Town"
            };

            context.Routes.Add(route);
            await context.SaveChangesAsync();

            var testEvent = new Event
            {
                OrganiserID = organiser.UserID,
                RouteID = route.RouteID,
                EventName = "Test Event",
                Description = "Test event",
                EventDate = DateTime.Now.AddDays(30),
                Location = "Cape Town",
                EntryFee = 100,
                MaximumParticipants = 50,
                Status = "Active",
                EventType = "Run"
            };

            context.Events.Add(testEvent);
            await context.SaveChangesAsync();

            var category = new Category
            {
                EventID = testEvent.EventID,
                CategoryName = "10KM Run",
                Description = "Test category"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var enrolment = new EventEnrolment
            {
                EventID = testEvent.EventID,
                UserID = participant.UserID,
                CategoryID = category.CategoryID,
                EnrolmentDate = DateTime.Now,
                Status = "Registered"
            };

            context.EventEnrolments.Add(enrolment);
            await context.SaveChangesAsync();

            var controller = new ResultsController(context);

            var httpContext = CreateHttpContext();

            httpContext.Session.SetInt32(
                "UserID",
                organiser.UserID);

            httpContext.Session.SetString(
                "Role",
                "Organiser");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var dto = new ResultDto
            {
                EnrolmentID = enrolment.EnrolmentID,
                FinishTime = new TimeSpan(0, 52, 30),
                Position = 1,
                ResultStatus = "Finished"
            };

            var result = await controller.CreateResult(dto);

            Assert.IsType<OkObjectResult>(result);

            var savedResult =
                await context.Results.FirstAsync();

            Assert.Equal(
                enrolment.EnrolmentID,
                savedResult.EnrolmentID);

            Assert.Equal(
                new TimeSpan(0, 52, 30),
                savedResult.FinishTime);

            Assert.Equal(
                1,
                savedResult.Position);

            Assert.Equal(
                "Finished",
                savedResult.ResultStatus);
        }

        // TEST 8
        [Fact]
        public async Task Participant_ShouldBeAbleToViewOwnResults()
        {
            using var context = CreateContext();

            var organiser = CreateUser(
                context,
                "organiser@test.com",
                "Organiser",
                "Password123!");

            var participant = CreateUser(
                context,
                "participant@test.com",
                "Participant",
                "Password123!");

            var route = new RaceDayAPI.Models.Route
            {
                RouteName = "Test Route",
                DistanceKm = 10,
                StartLocation = "Cape Town",
                FinishLocation = "Cape Town"
            };

            context.Routes.Add(route);
            await context.SaveChangesAsync();

            var testEvent = new Event
            {
                OrganiserID = organiser.UserID,
                RouteID = route.RouteID,
                EventName = "Test Event",
                Description = "Test event",
                EventDate = DateTime.Now.AddDays(30),
                Location = "Cape Town",
                EntryFee = 100,
                MaximumParticipants = 50,
                Status = "Active",
                EventType = "Run"
            };

            context.Events.Add(testEvent);
            await context.SaveChangesAsync();

            var category = new Category
            {
                EventID = testEvent.EventID,
                CategoryName = "10KM Run",
                Description = "Test category"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var enrolment = new EventEnrolment
            {
                EventID = testEvent.EventID,
                UserID = participant.UserID,
                CategoryID = category.CategoryID,
                EnrolmentDate = DateTime.Now,
                Status = "Registered"
            };

            context.EventEnrolments.Add(enrolment);
            await context.SaveChangesAsync();

            var savedResult = new Result
            {
                EnrolmentID = enrolment.EnrolmentID,
                FinishTime = new TimeSpan(0, 52, 30),
                Position = 1,
                ResultStatus = "Finished"
            };

            context.Results.Add(savedResult);
            await context.SaveChangesAsync();

            var controller = new ResultsController(context);

            var httpContext = CreateHttpContext();

            httpContext.Session.SetInt32(
                "UserID",
                participant.UserID);

            httpContext.Session.SetString(
                "Role",
                "Participant");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var result = await controller.GetMyResults();

            var okResult =
                Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);
        }
    }
}
