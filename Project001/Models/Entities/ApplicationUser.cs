using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Project001.Models.Entities;

public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }
    
    [StringLength(50)]
    public string PreferredLanguage { get; set; } = "en";
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}