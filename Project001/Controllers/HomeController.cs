using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project001.Data;
using Project001.Models;
using Project001.Models.Entities;
using Project001.Models.ViewModels;

namespace Project001.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                var dashboardData = await GetDashboardDataAsync(userId);
                return View(dashboardData);
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<DashboardViewModel> GetDashboardDataAsync(string userId)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            // Get user's bookings for today
            var todaysBookings = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.UserId == userId && 
                           b.StartDateTime.Date == today &&
                           b.Status == BookingStatus.Confirmed)
                .OrderBy(b => b.StartDateTime)
                .ToListAsync();

            // Get user's upcoming bookings (next 7 days)
            var upcomingBookings = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.UserId == userId && 
                           b.StartDateTime > DateTime.Now &&
                           b.StartDateTime <= DateTime.Now.AddDays(7) &&
                           b.Status == BookingStatus.Confirmed)
                .OrderBy(b => b.StartDateTime)
                .Take(5)
                .ToListAsync();

            // Get user's weekly stats
            var weeklyBookingsCount = await _context.Bookings
                .Where(b => b.UserId == userId && 
                           b.StartDateTime >= startOfWeek &&
                           b.StartDateTime < endOfWeek)
                .CountAsync();

            // Get system stats
            var totalRooms = await _context.Rooms.CountAsync(r => r.IsActive);
            var availableRooms = await GetAvailableRoomsCountAsync();
            var totalUsers = await _context.Users.CountAsync(u => u.IsActive);

            // Get popular time slots
            var popularTimeSlot = await GetPopularTimeSlotAsync();

            // Get recent activity (last 10 bookings across system)
            var recentActivity = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.CreatedAt >= DateTime.Today.AddDays(-7))
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .ToListAsync();

            return new DashboardViewModel
            {
                TodaysBookings = todaysBookings,
                UpcomingBookings = upcomingBookings,
                WeeklyBookingsCount = weeklyBookingsCount,
                TotalRooms = totalRooms,
                AvailableRooms = availableRooms,
                TotalUsers = totalUsers,
                PopularTimeSlot = popularTimeSlot,
                RecentActivity = recentActivity
            };
        }

        private async Task<int> GetAvailableRoomsCountAsync()
        {
            var now = DateTime.Now;
            var currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var nextHour = currentHour.AddHours(1);

            var bookedRoomIds = await _context.Bookings
                .Where(b => b.StartDateTime < nextHour &&
                           b.EndDateTime > currentHour &&
                           b.Status == BookingStatus.Confirmed)
                .Select(b => b.RoomId)
                .Distinct()
                .ToListAsync();

            return await _context.Rooms
                .CountAsync(r => r.IsActive && 
                                r.Status == RoomStatus.Available && 
                                !bookedRoomIds.Contains(r.Id));
        }

        private async Task<string> GetPopularTimeSlotAsync()
        {
            var popularHour = await _context.Bookings
                .Where(b => b.StartDateTime >= DateTime.Today.AddDays(-30))
                .GroupBy(b => b.StartDateTime.Hour)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            if (popularHour == 0) return "No data available";

            var startTime = DateTime.Today.AddHours(popularHour);
            var endTime = startTime.AddHours(1);
            return $"{startTime:h:mm tt} - {endTime:h:mm tt}";
        }
    }
}
