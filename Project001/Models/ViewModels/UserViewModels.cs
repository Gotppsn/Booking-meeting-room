using System.ComponentModel.DataAnnotations;
using Project001.Models.Entities;

namespace Project001.Models.ViewModels;

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public bool IsActive { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class UserDetailViewModel
{
    public string Id { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public bool IsActive { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public List<Booking> RecentBookings { get; set; } = new List<Booking>();
    public UserBookingStats BookingStats { get; set; } = new UserBookingStats();
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class CreateUserViewModel
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    public string? Role { get; set; }

    [StringLength(10)]
    public string PreferredLanguage { get; set; } = "en";
}

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(10)]
    public string PreferredLanguage { get; set; } = "en";

    public List<string> CurrentRoles { get; set; } = new List<string>();
    public List<string>? SelectedRoles { get; set; }
}

public class UserBookingStats
{
    public int TotalBookings { get; set; }
    public int UpcomingBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
}