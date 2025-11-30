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
    public async Task<IActionResult> Create(Event model, IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            ModelState.AddModelError("ImageUrl", "An image is required.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
            await imageFile.CopyToAsync(stream);

        model.ImageUrl = "/uploads/" + fileName;
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
    public async Task<IActionResult> Edit(int id, Event model, IFormFile? imageFile)
    {
        if (id != model.Id)
            return BadRequest();

        var ev = await _context.Events.FindAsync(id);
        if (ev == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        if (imageFile != null && imageFile.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            ev.ImageUrl = "/uploads/" + fileName;
        }

        ev.Title = model.Title;
        ev.Description = model.Description;
        ev.Category = model.Category;
        ev.Date = model.Date;
        ev.Country = model.Country;
        ev.Region = model.Region;
        ev.Address = model.Address;

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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User)!;

        var ev = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id && e.CreatorId == userId);

        if (ev == null)
            return NotFound();

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();

        return RedirectToAction("MyEvents");
    }
}