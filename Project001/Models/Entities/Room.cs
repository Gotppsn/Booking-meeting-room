using System.ComponentModel.DataAnnotations;

namespace Project001.Models.Entities;

public class Room
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? NameTh { get; set; }
    
    [StringLength(100)]
    public string? NameJa { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(500)]
    public string? DescriptionTh { get; set; }
    
    [StringLength(500)]
    public string? DescriptionJa { get; set; }
    
    [Range(1, 1000)]
    public int Capacity { get; set; }
    
    [StringLength(50)]
    public string? Floor { get; set; }
    
    [StringLength(100)]
    public string? Location { get; set; }
    
    public bool HasProjector { get; set; } = false;
    public bool HasWhiteboard { get; set; } = false;
    public bool HasVideoConference { get; set; } = false;
    public bool HasPhone { get; set; } = false;
    public bool HasComputer { get; set; } = false;
    
    [StringLength(500)]
    public string? Equipment { get; set; }
    
    [StringLength(500)]
    public string? ImageUrl { get; set; }
    
    [Range(0, 10000)]
    public decimal? HourlyRate { get; set; }
    
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public enum RoomStatus
{
    Available = 0,
    Maintenance = 1,
    Disabled = 2
}