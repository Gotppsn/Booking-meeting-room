using Project001.Models.Entities;

namespace Project001.Models.ViewModels;

public class DashboardViewModel
{
    public List<Booking> TodaysBookings { get; set; } = new List<Booking>();
    public List<Booking> UpcomingBookings { get; set; } = new List<Booking>();
    public List<Booking> RecentActivity { get; set; } = new List<Booking>();
    public int WeeklyBookingsCount { get; set; }
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int TotalUsers { get; set; }
    public string PopularTimeSlot { get; set; } = string.Empty;
}