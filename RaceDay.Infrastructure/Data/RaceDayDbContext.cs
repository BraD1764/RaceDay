using Microsoft.EntityFrameworkCore;
using RaceDay.Core.Entities;

namespace RaceDay.Infrastructure.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Weather> WeatherRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasMaxLength(20);
            });

            // Organiser configuration
            modelBuilder.Entity<Organiser>(entity =>
            {
                entity.HasOne(o => o.User)
                    .WithOne(u => u.Organiser)
                    .HasForeignKey<Organiser>(o => o.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Participant configuration
            modelBuilder.Entity<Participant>(entity =>
            {
                entity.HasOne(p => p.User)
                    .WithOne(u => u.Participant)
                    .HasForeignKey<Participant>(p => p.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Event configuration
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasOne(e => e.Organiser)
                    .WithMany(o => o.Events)
                    .HasForeignKey(e => e.OrganiserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasOne(c => c.Event)
                    .WithMany(e => e.Categories)
                    .HasForeignKey(c => c.EventID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Enrolment configuration
            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.HasOne(e => e.Participant)
                    .WithMany(p => p.Enrolments)
                    .HasForeignKey(e => e.ParticipantID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Enrolments)
                    .HasForeignKey(e => e.CategoryID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ParticipantID, e.CategoryID }).IsUnique();
            });

            // Result configuration
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasOne(r => r.Enrolment)
                    .WithOne(e => e.Result)
                    .HasForeignKey<Result>(r => r.EnrolmentID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(r => r.EnrolmentID).IsUnique();
            });

            // Weather configuration
            modelBuilder.Entity<Weather>(entity =>
            {
                entity.HasOne(w => w.Event)
                    .WithMany(e => e.WeatherRecords)
                    .HasForeignKey(w => w.EventID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(w => new { w.EventID, w.ForecastDate }).IsUnique();
            });
        }
    }
}