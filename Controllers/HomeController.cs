using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicTacToeApp.Models;
using TicTacToeApp.Data;

namespace TicTacToeApp.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (Request.Cookies.ContainsKey("PlayerId"))
        {
            return RedirectToAction("Index", "Lobby");
        }
        return View();
    }

    [HttpPost]
    public IActionResult Index(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            ModelState.AddModelError("", "Please enter a valid name.");
            return View();
        }

        var player = _context.Players.FirstOrDefault(p => p.Name == playerName);
        if (player == null)
        {
            player = new Player { Name = playerName };
            _context.Players.Add(player);
            _context.SaveChanges();
        }

        Response.Cookies.Append("PlayerId", player.Id.ToString(), new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7) });
        Response.Cookies.Append("PlayerName", player.Name, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7) });

        return RedirectToAction("Index", "Lobby");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("PlayerId");
        Response.Cookies.Delete("PlayerName");
        return RedirectToAction("Index");
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
