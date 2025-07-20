using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project001.Data;
using Project001.Models.Entities;

namespace Project001.Controllers;

[Authorize]
public class RoomsController : Controller
{
    private readonly ApplicationDbContext _context;

    public RoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Rooms
    public async Task<IActionResult> Index(string? search, int? capacity, bool? hasProjector, bool? hasVideoConference)
    {
        var rooms = _context.Rooms.Where(r => r.IsActive);

        if (!string.IsNullOrEmpty(search))
        {
            rooms = rooms.Where(r => r.Name.Contains(search) || 
                                   (r.Location != null && r.Location.Contains(search)) ||
                                   (r.Floor != null && r.Floor.Contains(search)));
        }

        if (capacity.HasValue)
        {
            rooms = rooms.Where(r => r.Capacity >= capacity.Value);
        }

        if (hasProjector.HasValue && hasProjector.Value)
        {
            rooms = rooms.Where(r => r.HasProjector);
        }

        if (hasVideoConference.HasValue && hasVideoConference.Value)
        {
            rooms = rooms.Where(r => r.HasVideoConference);
        }

        ViewData["Search"] = search;
        ViewData["Capacity"] = capacity;
        ViewData["HasProjector"] = hasProjector;
        ViewData["HasVideoConference"] = hasVideoConference;

        return View(await rooms.OrderBy(r => r.Name).ToListAsync());
    }

    // GET: Rooms/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms
            .Include(r => r.Bookings.Where(b => b.StartDateTime >= DateTime.Today && b.Status == BookingStatus.Confirmed))
            .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (room == null)
        {
            return NotFound();
        }

        return View(room);
    }

    // GET: Rooms/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Rooms/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Room room)
    {
        if (ModelState.IsValid)
        {
            room.CreatedAt = DateTime.UtcNow;
            room.UpdatedAt = DateTime.UtcNow;
            _context.Add(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(room);
    }

    // GET: Rooms/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    // POST: Rooms/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Room room)
    {
        if (id != room.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                room.UpdatedAt = DateTime.UtcNow;
                _context.Update(room);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(room.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(room);
    }

    // GET: Rooms/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms
            .FirstOrDefaultAsync(m => m.Id == id);
        if (room == null)
        {
            return NotFound();
        }

        return View(room);
    }

    // POST: Rooms/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room != null)
        {
            room.IsActive = false;
            room.UpdatedAt = DateTime.UtcNow;
            _context.Update(room);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Rooms/Availability/5
    public async Task<IActionResult> Availability(int id, DateTime? date)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        var selectedDate = date ?? DateTime.Today;
        var startOfDay = selectedDate.Date;
        var endOfDay = startOfDay.AddDays(1);

        var bookings = await _context.Bookings
            .Where(b => b.RoomId == id && 
                       b.StartDateTime < endOfDay && 
                       b.EndDateTime > startOfDay && 
                       b.Status == BookingStatus.Confirmed)
            .Include(b => b.User)
            .OrderBy(b => b.StartDateTime)
            .ToListAsync();

        ViewData["Room"] = room;
        ViewData["SelectedDate"] = selectedDate;
        return View(bookings);
    }

    private bool RoomExists(int id)
    {
        return _context.Rooms.Any(e => e.Id == id);
    }
}