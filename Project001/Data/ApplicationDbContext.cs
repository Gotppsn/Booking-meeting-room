using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project001.Models.Entities;

namespace Project001.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Department)
                  .WithMany(d => d.Users)
                  .HasForeignKey(u => u.DepartmentId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(u => u.Email).IsUnique();
        });

        builder.Entity<Room>(entity =>
        {
            entity.HasIndex(r => r.Name);
            entity.Property(r => r.HourlyRate).HasPrecision(10, 2);
        });

        builder.Entity<Booking>(entity =>
        {
            entity.HasOne(b => b.Room)
                  .WithMany(r => r.Bookings)
                  .HasForeignKey(b => b.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.User)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.ParentBooking)
                  .WithMany(b => b.RecurringBookings)
                  .HasForeignKey(b => b.ParentBookingId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(b => new { b.RoomId, b.StartDateTime, b.EndDateTime });
            entity.HasIndex(b => b.StartDateTime);
            entity.HasIndex(b => b.Status);
        });

        builder.Entity<Department>(entity =>
        {
            entity.HasIndex(d => d.Name);
        });

        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        builder.Entity<Department>().HasData(
            new Department
            {
                Id = 1,
                Name = "Administration",
                NameTh = "ฝ่ายบริหาร",
                NameJa = "管理部",
                ColorCode = "#2563EB",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Department
            {
                Id = 2,
                Name = "Information Technology",
                NameTh = "ฝ่ายเทคโนโลยีสารสนเทศ",
                NameJa = "情報技術部",
                ColorCode = "#10B981",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Department
            {
                Id = 3,
                Name = "Human Resources",
                NameTh = "ฝ่ายทรัพยากรบุคคล",
                NameJa = "人事部",
                ColorCode = "#F59E0B",
                IsActive = true,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );
    }
}