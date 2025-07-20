using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project001.Data;
using Project001.Models.Entities;
using Project001.Services;

namespace Project001.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public BookingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
    }

    // GET: Bookings
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, BookingStatus? status)
    {
        var currentUserId = _userManager.GetUserId(User);
        var query = _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .Where(b => b.UserId == currentUserId);

        if (startDate.HasValue)
        {
            query = query.Where(b => b.StartDateTime.Date >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(b => b.StartDateTime.Date <= endDate.Value.Date);
        }

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
        ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
        ViewData["Status"] = status;

        var bookings = await query.OrderByDescending(b => b.StartDateTime).ToListAsync();
        return View(bookings);
    }

    // GET: Bookings/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .Include(b => b.RecurringBookings)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        // Check if user owns this booking or is admin
        var currentUserId = _userManager.GetUserId(User);
        if (booking.UserId != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(booking);
    }

    // GET: Bookings/Create
    public async Task<IActionResult> Create(int? roomId, DateTime? startTime, string? date)
    {
        ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.IsActive && r.Status == RoomStatus.Available).ToListAsync(), "Id", "Name", roomId);
        
        var booking = new Booking();
        
        if (startTime.HasValue)
        {
            booking.StartDateTime = startTime.Value;
            booking.EndDateTime = startTime.Value.AddHours(1);
        }
        else if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var selectedDate))
        {
            booking.StartDateTime = selectedDate.Date.AddHours(9); // Default to 9 AM
            booking.EndDateTime = selectedDate.Date.AddHours(10);  // Default to 10 AM
        }
        else
        {
            booking.StartDateTime = DateTime.Today.AddDays(1).AddHours(9);
            booking.EndDateTime = DateTime.Today.AddDays(1).AddHours(10);
        }

        if (roomId.HasValue)
        {
            booking.RoomId = roomId.Value;
        }

        return View(booking);
    }

    // POST: Bookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Booking booking)
    {
        var currentUserId = _userManager.GetUserId(User);
        booking.UserId = currentUserId;
        booking.CreatedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        // Validate booking times
        if (booking.StartDateTime >= booking.EndDateTime)
        {
            ModelState.AddModelError("EndDateTime", "End time must be after start time.");
        }

        if (booking.StartDateTime < DateTime.Now)
        {
            ModelState.AddModelError("StartDateTime", "Cannot book in the past.");
        }

        // Check for conflicts
        var hasConflict = await _context.Bookings.AnyAsync(b =>
            b.RoomId == booking.RoomId &&
            b.Status == BookingStatus.Confirmed &&
            b.StartDateTime < booking.EndDateTime &&
            b.EndDateTime > booking.StartDateTime);

        if (hasConflict)
        {
            ModelState.AddModelError("", "This time slot conflicts with an existing booking.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(booking);
            await _context.SaveChangesAsync();

            // Load the booking with related data for email
            booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstAsync(b => b.Id == booking.Id);

            // Send confirmation email
            await _emailService.SendBookingConfirmationAsync(booking);

            // Handle recurring bookings
            if (booking.RecurrenceType.HasValue && booking.RecurrenceType != RecurrenceType.None && booking.RecurrenceEnd.HasValue)
            {
                await CreateRecurringBookings(booking);
            }

            return RedirectToAction(nameof(Details), new { id = booking.Id });
        }

        ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.IsActive).ToListAsync(), "Id", "Name", booking.RoomId);
        return View(booking);
    }

    // GET: Bookings/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        // Check if user owns this booking or is admin
        var currentUserId = _userManager.GetUserId(User);
        if (booking.UserId != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.IsActive).ToListAsync(), "Id", "Name", booking.RoomId);
        return View(booking);
    }

    // POST: Bookings/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Booking booking)
    {
        if (id != booking.Id)
        {
            return NotFound();
        }

        var existingBooking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (existingBooking == null)
        {
            return NotFound();
        }

        // Check if user owns this booking or is admin
        var currentUserId = _userManager.GetUserId(User);
        if (existingBooking.UserId != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        booking.UserId = existingBooking.UserId;
        booking.CreatedAt = existingBooking.CreatedAt;
        booking.UpdatedAt = DateTime.UtcNow;

        // Validate booking times
        if (booking.StartDateTime >= booking.EndDateTime)
        {
            ModelState.AddModelError("EndDateTime", "End time must be after start time.");
        }

        // Check for conflicts (excluding current booking)
        var hasConflict = await _context.Bookings.AnyAsync(b =>
            b.Id != booking.Id &&
            b.RoomId == booking.RoomId &&
            b.Status == BookingStatus.Confirmed &&
            b.StartDateTime < booking.EndDateTime &&
            b.EndDateTime > booking.StartDateTime);

        if (hasConflict)
        {
            ModelState.AddModelError("", "This time slot conflicts with an existing booking.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();

                // Load the booking with related data for email
                booking = await _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.User)
                    .FirstAsync(b => b.Id == booking.Id);

                // Send update email
                await _emailService.SendBookingUpdateAsync(booking);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Details), new { id = booking.Id });
        }

        ViewData["RoomId"] = new SelectList(await _context.Rooms.Where(r => r.IsActive).ToListAsync(), "Id", "Name", booking.RoomId);
        return View(booking);
    }

    // POST: Bookings/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        // Check if user owns this booking or is admin
        var currentUserId = _userManager.GetUserId(User);
        if (booking.UserId != currentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = reason;
        booking.UpdatedAt = DateTime.UtcNow;

        _context.Update(booking);
        await _context.SaveChangesAsync();

        // Load the booking with related data for email
        booking = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .FirstAsync(b => b.Id == booking.Id);

        // Send cancellation email
        await _emailService.SendBookingCancellationAsync(booking);

        return RedirectToAction(nameof(Index));
    }

    // GET: Bookings/Calendar
    public async Task<IActionResult> Calendar(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;
        var startOfWeek = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .Where(b => b.StartDateTime >= startOfWeek && b.StartDateTime < endOfWeek && b.Status == BookingStatus.Confirmed)
            .OrderBy(b => b.StartDateTime)
            .ToListAsync();

        var rooms = await _context.Rooms.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();

        ViewData["SelectedDate"] = selectedDate;
        ViewData["StartOfWeek"] = startOfWeek;
        ViewData["Rooms"] = rooms;
        ViewData["Bookings"] = bookings;

        return View();
    }

    // GET: API endpoint for calendar data
    [HttpGet]
    public async Task<IActionResult> GetCalendarData(DateTime start, DateTime end)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .Where(b => b.StartDateTime >= start && b.StartDateTime <= end && b.Status == BookingStatus.Confirmed)
            .Select(b => new
            {
                id = b.Id,
                title = b.Title,
                start = b.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = b.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                room = b.Room.Name,
                user = b.User.FullName,
                description = b.Description
            })
            .ToListAsync();

        return Json(bookings);
    }

    private async Task CreateRecurringBookings(Booking parentBooking)
    {
        var bookings = new List<Booking>();
        var currentDate = parentBooking.StartDateTime;
        var duration = parentBooking.EndDateTime - parentBooking.StartDateTime;

        while (currentDate <= parentBooking.RecurrenceEnd)
        {
            switch (parentBooking.RecurrenceType)
            {
                case RecurrenceType.Daily:
                    currentDate = currentDate.AddDays(1);
                    break;
                case RecurrenceType.Weekly:
                    currentDate = currentDate.AddDays(7);
                    break;
                case RecurrenceType.Monthly:
                    currentDate = currentDate.AddMonths(1);
                    break;
                default:
                    return;
            }

            if (currentDate > parentBooking.RecurrenceEnd)
                break;

            // Check for conflicts
            var hasConflict = await _context.Bookings.AnyAsync(b =>
                b.RoomId == parentBooking.RoomId &&
                b.Status == BookingStatus.Confirmed &&
                b.StartDateTime < currentDate.Add(duration) &&
                b.EndDateTime > currentDate);

            if (!hasConflict)
            {
                bookings.Add(new Booking
                {
                    RoomId = parentBooking.RoomId,
                    UserId = parentBooking.UserId,
                    Title = parentBooking.Title,
                    Description = parentBooking.Description,
                    StartDateTime = currentDate,
                    EndDateTime = currentDate.Add(duration),
                    Attendees = parentBooking.Attendees,
                    AttendeeCount = parentBooking.AttendeeCount,
                    Status = BookingStatus.Confirmed,
                    ParentBookingId = parentBooking.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        if (bookings.Any())
        {
            _context.Bookings.AddRange(bookings);
            await _context.SaveChangesAsync();
        }
    }

    private bool BookingExists(int id)
    {
        return _context.Bookings.Any(e => e.Id == id);
    }
}