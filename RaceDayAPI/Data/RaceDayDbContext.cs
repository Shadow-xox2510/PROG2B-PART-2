using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RaceDayAPI.Models.Route> Routes { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<EventEnrolment> EventEnrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserID);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<User>()
                .Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            // Routes
            modelBuilder.Entity<RaceDayAPI.Models.Route>()
                .HasKey(r => r.RouteID);

            modelBuilder.Entity<RaceDayAPI.Models.Route>()
                .Property(r => r.RouteName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<RaceDayAPI.Models.Route>()
                .Property(r => r.DistanceKm)
                .HasPrecision(6, 2);

            modelBuilder.Entity<RaceDayAPI.Models.Route>()
                .Property(r => r.StartLocation)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<RaceDayAPI.Models.Route>()
                .Property(r => r.FinishLocation)
                .IsRequired()
                .HasMaxLength(150);

            // Events
            modelBuilder.Entity<Event>()
                .HasKey(e => e.EventID);

            modelBuilder.Entity<Event>()
                .Property(e => e.EventName)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<Event>()
                .Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<Event>()
                .Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<Event>()
                .Property(e => e.EntryFee)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Event>()
                .Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Event>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne<RaceDayAPI.Models.Route>()
                .WithMany()
                .HasForeignKey(e => e.RouteID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(20);

            // Categories
            modelBuilder.Entity<Category>()
                .HasKey(c => c.CategoryID);

            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Category>()
                .Property(c => c.Description)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Category>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            // Event Enrolments
            modelBuilder.Entity<EventEnrolment>()
                .HasKey(e => e.EnrolmentID);

            modelBuilder.Entity<EventEnrolment>()
                .Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<EventEnrolment>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventEnrolment>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventEnrolment>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Results
            modelBuilder.Entity<Result>()
                .HasKey(r => r.ResultID);

            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentID)
                .IsUnique();

            modelBuilder.Entity<Result>()
                .Property(r => r.ResultStatus)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Result>()
                .HasOne<EventEnrolment>()
                .WithMany()
                .HasForeignKey(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}