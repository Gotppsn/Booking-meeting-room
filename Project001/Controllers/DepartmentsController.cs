using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project001.Data;
using Project001.Models.Entities;

namespace Project001.Controllers;

[Authorize(Roles = "Admin")]
public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public DepartmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Departments
    public async Task<IActionResult> Index(string? search, bool? isActive)
    {
        var query = _context.Departments.Include(d => d.Users).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d => d.Name.Contains(search) || 
                                   (d.NameTh != null && d.NameTh.Contains(search)) ||
                                   (d.NameJa != null && d.NameJa.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(d => d.IsActive == isActive.Value);
        }

        ViewData["Search"] = search;
        ViewData["IsActive"] = isActive;

        return View(await query.OrderBy(d => d.Name).ToListAsync());
    }

    // GET: Departments/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await _context.Departments
            .Include(d => d.Users.Where(u => u.IsActive))
            .FirstOrDefaultAsync(m => m.Id == id);

        if (department == null)
        {
            return NotFound();
        }

        // Get department statistics
        var departmentUsers = department.Users.ToList();
        var userIds = departmentUsers.Select(u => u.Id).ToList();
        
        var bookingStats = await _context.Bookings
            .Where(b => userIds.Contains(b.UserId))
            .GroupBy(b => 1)
            .Select(g => new
            {
                TotalBookings = g.Count(),
                UpcomingBookings = g.Count(b => b.StartDateTime > DateTime.Now && b.Status == BookingStatus.Confirmed),
                ThisMonthBookings = g.Count(b => b.StartDateTime.Month == DateTime.Now.Month && b.StartDateTime.Year == DateTime.Now.Year)
            })
            .FirstOrDefaultAsync();

        ViewData["BookingStats"] = bookingStats ?? new { TotalBookings = 0, UpcomingBookings = 0, ThisMonthBookings = 0 };

        return View(department);
    }

    // GET: Departments/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Departments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Department department)
    {
        if (ModelState.IsValid)
        {
            department.CreatedAt = DateTime.UtcNow;
            department.UpdatedAt = DateTime.UtcNow;
            
            // Ensure color code has # prefix
            if (!string.IsNullOrEmpty(department.ColorCode) && !department.ColorCode.StartsWith("#"))
            {
                department.ColorCode = "#" + department.ColorCode;
            }

            _context.Add(department);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    // GET: Departments/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }
        return View(department);
    }

    // POST: Departments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Department department)
    {
        if (id != department.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingDepartment = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
                if (existingDepartment == null)
                {
                    return NotFound();
                }

                department.CreatedAt = existingDepartment.CreatedAt;
                department.UpdatedAt = DateTime.UtcNow;
                
                // Ensure color code has # prefix
                if (!string.IsNullOrEmpty(department.ColorCode) && !department.ColorCode.StartsWith("#"))
                {
                    department.ColorCode = "#" + department.ColorCode;
                }

                _context.Update(department);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(department.Id))
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
        return View(department);
    }

    // GET: Departments/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await _context.Departments
            .Include(d => d.Users)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    // POST: Departments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var department = await _context.Departments.Include(d => d.Users).FirstOrDefaultAsync(d => d.Id == id);
        if (department != null)
        {
            // Check if department has users
            if (department.Users.Any())
            {
                // Soft delete - mark as inactive
                department.IsActive = false;
                department.UpdatedAt = DateTime.UtcNow;
                _context.Update(department);
            }
            else
            {
                // Hard delete if no users
                _context.Departments.Remove(department);
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: Departments/ToggleActive/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        department.IsActive = !department.IsActive;
        department.UpdatedAt = DateTime.UtcNow;

        _context.Update(department);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }

    // GET: Departments/Stats
    public async Task<IActionResult> Stats()
    {
        var departments = await _context.Departments
            .Include(d => d.Users)
            .Where(d => d.IsActive)
            .ToListAsync();

        var stats = new List<object>();

        foreach (var dept in departments)
        {
            var userIds = dept.Users.Where(u => u.IsActive).Select(u => u.Id).ToList();
            
            var bookingCount = await _context.Bookings
                .Where(b => userIds.Contains(b.UserId))
                .CountAsync();

            var upcomingBookings = await _context.Bookings
                .Where(b => userIds.Contains(b.UserId) && 
                           b.StartDateTime > DateTime.Now && 
                           b.Status == BookingStatus.Confirmed)
                .CountAsync();

            stats.Add(new
            {
                DepartmentId = dept.Id,
                DepartmentName = dept.Name,
                ColorCode = dept.ColorCode,
                UserCount = dept.Users.Count(u => u.IsActive),
                TotalBookings = bookingCount,
                UpcomingBookings = upcomingBookings
            });
        }

        return View(stats);
    }

    private bool DepartmentExists(int id)
    {
        return _context.Departments.Any(e => e.Id == id);
    }
}