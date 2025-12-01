using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Townsquare.Data;
using Townsquare.Models;
using System.Net.Http.Json;

namespace Townsquare.Controllers;

public class CountryResponse
{
    public Name name { get; set; }
}

public class Name
{
    public string common { get; set; }
}


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

    public async Task<IActionResult> Create()
    {
        ViewBag.Countries = await GetCountriesAsync();
        return View();
    }

    
    private async Task<List<string>> GetCountriesAsync()
    {
        var http = new HttpClient();

        var data = await http.GetFromJsonAsync<List<CountryResponse>>(
            "https://restcountries.com/v3.1/all?fields=name"
        );

        return data!
            .Select(c => c.name.common)
            .OrderBy(n => n)
            .ToList();
    }

    
    [HttpPost]
    public async Task<IActionResult> Create(Event model, IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            ModelState.AddModelError("ImageUrl", "An image is required.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Countries = await GetCountriesAsync();
            return View(model);
        }


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

        ViewBag.Countries = await GetCountriesAsync();
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
        {
            ViewBag.Countries = await GetCountriesAsync();
            return View(model);
        }


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
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RSVP(int id)
    {
        var userId = _userManager.GetUserId(User)!;

        var already = await _context.Rsvps
            .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == userId);

        if (already == null)
        {
            var ev = await _context.Events
                .Include(e => e.Creator)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            _context.Rsvps.Add(new RSVP
            {
                EventId = id,
                UserId = userId
            });

            _context.Notifications.Add(new Notification
            {
                UserId = ev.CreatorId,
                Title = $"New RSVP - {ev.Title}",
                Description = $"{User.Identity.Name} joined your event."
            });


            await _context.SaveChangesAsync();

        }

        return RedirectToAction("Details", new { id });
    }


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CancelRSVP(int id)
    {
        var userId = _userManager.GetUserId(User)!;

        var rsvp = await _context.Rsvps
            .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == userId);

        if (rsvp != null)
        {
            _context.Rsvps.Remove(rsvp);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", new { id });
    }
    
    [Authorize]
    public async Task<IActionResult> Attendees(int id)
    {
        var ev = await _context.Events
            .Include(e => e.Rsvps)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null)
            return NotFound();

        return View(ev);
    }

}