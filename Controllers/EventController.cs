using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Townsquare.Data;
using Townsquare.Models;

namespace Townsquare.Controllers;

[Authorize]
public class EventController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EventController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(Event model)
    {
        if (!ModelState.IsValid)
            return View(model);

        model.CreatorId = _userManager.GetUserId(User)!;

        _context.Events.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("MyEvents");
    }
    
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var ev = await _context.Events
            .Include(e => e.Creator)
            .Include(e => e.Rsvps)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null)
            return NotFound();

        return View(ev);
    }
    
    public async Task<IActionResult> Edit(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null)
            return NotFound();

        if (ev.CreatorId != _userManager.GetUserId(User))
            return Forbid();

        return View(ev);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Event model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var ev = await _context.Events.FindAsync(id);
        if (ev == null)
            return NotFound();

        if (ev.CreatorId != _userManager.GetUserId(User))
            return Forbid();

        ev.Title = model.Title;
        ev.Description = model.Description;
        ev.Category = model.Category;
        ev.ImageUrl = model.ImageUrl;
        ev.Date = model.Date;
        ev.Country = model.Country;
        ev.Region = model.Region;
        ev.Address = model.Address;

        await _context.SaveChangesAsync();

        return RedirectToAction("MyEvents");
    }
    
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null)
            return NotFound();

        if (ev.CreatorId != _userManager.GetUserId(User))
            return Forbid();

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();

        return RedirectToAction("MyEvents");
    }

    public async Task<IActionResult> MyEvents()
    {
        var userId = _userManager.GetUserId(User)!;

        var eventsList = await _context.Events
            .Where(e => e.CreatorId == userId)
            .Include(e => e.Rsvps)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        return View(eventsList);
    }
    
    [HttpPost]
    public async Task<IActionResult> DeleteMany(int[] ids)
    {
        var userId = _userManager.GetUserId(User)!;

        var eventsToDelete = await _context.Events
            .Where(e => ids.Contains(e.Id) && e.CreatorId == userId)
            .ToListAsync();

        if (eventsToDelete.Count == 0)
            return RedirectToAction("MyEvents");

        _context.Events.RemoveRange(eventsToDelete);
        await _context.SaveChangesAsync();

        return RedirectToAction("MyEvents");
    }

}