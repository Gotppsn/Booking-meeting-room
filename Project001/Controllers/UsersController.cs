using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project001.Data;
using Project001.Models.Entities;
using Project001.Models.ViewModels;

namespace Project001.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Users
    public async Task<IActionResult> Index(string? search, int? departmentId, string? role, bool? isActive)
    {
        var query = _context.Users.Include(u => u.Department).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FirstName.Contains(search) || 
                                   u.LastName.Contains(search) || 
                                   u.Email.Contains(search) ||
                                   u.UserName.Contains(search));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == departmentId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var users = await query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToListAsync();

        // Get user roles
        var userViewModels = new List<UserViewModel>();
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var isInRole = string.IsNullOrEmpty(role) || userRoles.Contains(role);
            
            if (isInRole)
            {
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    DepartmentId = user.DepartmentId,
                    Department = user.Department,
                    IsActive = user.IsActive,
                    PreferredLanguage = user.PreferredLanguage,
                    CreatedAt = user.CreatedAt,
                    Roles = userRoles.ToList()
                });
            }
        }

        ViewData["Search"] = search;
        ViewData["DepartmentId"] = new SelectList(await _context.Departments.Where(d => d.IsActive).ToListAsync(), "Id", "Name", departmentId);
        ViewData["Role"] = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", role);
        ViewData["IsActive"] = isActive;

        return View(userViewModels);
    }

    // GET: Users/Details/5
    public async Task<IActionResult> Details(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Bookings.Where(b => b.StartDateTime >= DateTime.Today.AddDays(-30)))
            .ThenInclude(b => b.Room)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var recentBookings = user.Bookings.OrderByDescending(b => b.CreatedAt).Take(10).ToList();

        var viewModel = new UserDetailViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            UserName = user.UserName,
            DepartmentId = user.DepartmentId,
            Department = user.Department,
            IsActive = user.IsActive,
            PreferredLanguage = user.PreferredLanguage,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = userRoles.ToList(),
            RecentBookings = recentBookings,
            BookingStats = new UserBookingStats
            {
                TotalBookings = user.Bookings.Count,
                UpcomingBookings = user.Bookings.Count(b => b.StartDateTime > DateTime.Now && b.Status == BookingStatus.Confirmed),
                CompletedBookings = user.Bookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = user.Bookings.Count(b => b.Status == BookingStatus.Cancelled)
            }
        };

        return View(viewModel);
    }

    // GET: Users/Create
    public async Task<IActionResult> Create()
    {
        ViewData["DepartmentId"] = new SelectList(await _context.Departments.Where(d => d.IsActive).ToListAsync(), "Id", "Name");
        ViewData["Roles"] = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
        return View();
    }

    // POST: Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DepartmentId = model.DepartmentId,
                PreferredLanguage = model.PreferredLanguage,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewData["DepartmentId"] = new SelectList(await _context.Departments.Where(d => d.IsActive).ToListAsync(), "Id", "Name", model.DepartmentId);
        ViewData["Roles"] = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", model.Role);
        return View(model);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            DepartmentId = user.DepartmentId,
            PreferredLanguage = user.PreferredLanguage,
            IsActive = user.IsActive,
            CurrentRoles = userRoles.ToList()
        };

        ViewData["DepartmentId"] = new SelectList(await _context.Departments.Where(d => d.IsActive).ToListAsync(), "Id", "Name", model.DepartmentId);
        ViewData["Roles"] = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
        return View(model);
    }

    // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.DepartmentId = model.DepartmentId;
            user.PreferredLanguage = model.PreferredLanguage;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var selectedRoles = model.SelectedRoles ?? new List<string>();

                var rolesToRemove = currentRoles.Except(selectedRoles);
                var rolesToAdd = selectedRoles.Except(currentRoles);

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }

                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewData["DepartmentId"] = new SelectList(await _context.Departments.Where(d => d.IsActive).ToListAsync(), "Id", "Name", model.DepartmentId);
        ViewData["Roles"] = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
        return View(model);
    }

    // POST: Users/ToggleActive/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        return RedirectToAction(nameof(Index));
    }

    // POST: Users/ResetPassword/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            return Json(new { success = true, message = "Password reset successfully" });
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return Json(new { success = false, message = errors });
    }

    // GET: Users/Export
    public async Task<IActionResult> Export()
    {
        var users = await _context.Users
            .Include(u => u.Department)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        // Simple CSV export
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("First Name,Last Name,Email,Department,Active,Created");

        foreach (var user in users)
        {
            csv.AppendLine($"{user.FirstName},{user.LastName},{user.Email},{user.Department?.Name},{user.IsActive},{user.CreatedAt:yyyy-MM-dd}");
        }

        return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "users.csv");
    }
}