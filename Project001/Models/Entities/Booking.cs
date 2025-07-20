using System.ComponentModel.DataAnnotations;

namespace Project001.Models.Entities;

public class Booking
{
    public int Id { get; set; }
    
    public int RoomId { get; set; }
    public virtual Room Room { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    
    [StringLength(500)]
    public string? Attendees { get; set; }
    
    public int? AttendeeCount { get; set; }
    
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    
    public RecurrenceType? RecurrenceType { get; set; }
    public DateTime? RecurrenceEnd { get; set; }
    public int? ParentBookingId { get; set; }
    public virtual Booking? ParentBooking { get; set; }
    public virtual ICollection<Booking> RecurringBookings { get; set; } = new List<Booking>();
    
    [StringLength(1000)]
    public string? CancellationReason { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsRecurring => ParentBookingId.HasValue;
    public TimeSpan Duration => EndDateTime - StartDateTime;
}

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}

public enum RecurrenceType
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}