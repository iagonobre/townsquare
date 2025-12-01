using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Townsquare.Data;
using Townsquare.Models;

namespace Townsquare.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _context;

    public AdminController(UserManager<ApplicationUser> users, ApplicationDbContext context)
    {
        _users = users;
        _context = context;
    }

    public async Task<IActionResult> Users()
    {
        var data = await _users.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        if (await _users.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = "Admin accounts cannot be deleted.";
            return RedirectToAction("Users");
        }

        var currentUserId = _users.GetUserId(User);
        if (user.Id == currentUserId)
        {
            TempData["Error"] = "You cannot delete your own admin account.";
            return RedirectToAction("Users");
        }

        var events = await _context.Events
            .Where(e => e.CreatorId == id)
            .ToListAsync();

        foreach (var ev in events)
        {
            ev.CreatorId = null;
            ev.Creator = null;
            ev.IsOrphaned = true;
        }

        await _context.SaveChangesAsync();

        user.IsDeleted = true;
        await _users.UpdateAsync(user);

        TempData["Success"] = "User deleted successfully.";
        return RedirectToAction("Users");
    }

}