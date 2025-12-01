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

    public async Task<IActionResult> Index(string? search, string? category, string? country, string? region)
    {
        var eventsQuery = _context.Events
            .OrderBy(e => e.Date)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            eventsQuery = eventsQuery.Where(e => e.Title.Contains(search));

        if (!string.IsNullOrWhiteSpace(category))
            eventsQuery = eventsQuery.Where(e => e.Category == category);

        if (!string.IsNullOrWhiteSpace(country))
            eventsQuery = eventsQuery.Where(e => e.Country == country);

        if (!string.IsNullOrWhiteSpace(region))
            eventsQuery = eventsQuery.Where(e => e.Region == region);

        var eventsList = eventsQuery.ToList();

        ViewBag.Countries = _context.Events
            .Select(e => e.Country)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        ViewBag.Regions = _context.Events
            .Select(e => e.Region)
            .Where(r => !string.IsNullOrEmpty(r))
            .Distinct()
            .OrderBy(r => r)
            .ToList();

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