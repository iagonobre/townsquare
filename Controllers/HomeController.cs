using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Townsquare.Data;
using Townsquare.Models;

namespace Townsquare.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index(string? search, string? category, string? city)
    {
        var eventsQuery = _context.Events
            .Include(e => e.Rsvps)    
            .OrderBy(e => e.Date)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            eventsQuery = eventsQuery.Where(e => e.Title.Contains(search));

        if (!string.IsNullOrWhiteSpace(category))
            eventsQuery = eventsQuery.Where(e => e.Category == category);

        if (!string.IsNullOrWhiteSpace(city))
            eventsQuery = eventsQuery.Where(e => e.Country == city);

        var eventsList = eventsQuery.ToList();

        var cities = _context.Events
            .Select(e => e.Country)
            .Where(c => c != null && c != "")
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        ViewBag.Cities = cities;

        return View(eventsList);
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
}