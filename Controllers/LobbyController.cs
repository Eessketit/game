using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TicTacToeApp.Data;
using TicTacToeApp.Hubs;
using TicTacToeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace TicTacToeApp.Controllers
{
    public class LobbyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<GameHub> _hubContext;

        public LobbyController(AppDbContext context, IHubContext<GameHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            var playerIdStr = Request.Cookies["PlayerId"];
            if (string.IsNullOrEmpty(playerIdStr) || !int.TryParse(playerIdStr, out int playerId))
            {
                return RedirectToAction("Index", "Home");
            }

            var player = _context.Players.Find(playerId);
            if (player == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.PlayerName = player.Name;
            ViewBag.Wins = player.Wins;
            ViewBag.Losses = player.Losses;
            ViewBag.Draws = player.Draws;

            // Get active waiting games
            var activeGames = _context.GameSessions
                .Include(g => g.Player1)
                .Where(g => g.Status == "Waiting" && g.Player1Id != null)
                .Select(g => new { 
                    g.Id, 
                    CreatorName = g.Player1.Name 
                }).ToList();

            ViewBag.ActiveGames = activeGames;

            return View();
        }

        [HttpPost]
        public IActionResult CreateGame()
        {
            var playerIdStr = Request.Cookies["PlayerId"];
            if (string.IsNullOrEmpty(playerIdStr) || !int.TryParse(playerIdStr, out int playerId))
            {
                return RedirectToAction("Index", "Home");
            }

            var gameSession = new GameSession
            {
                Id = Guid.NewGuid().ToString(),
                Status = "Waiting",
                Player1Id = playerId
            };

            _context.GameSessions.Add(gameSession);
            _context.SaveChanges();

            return RedirectToAction("Game", new { id = gameSession.Id });
        }
        
        [HttpPost]
        public async Task<IActionResult> JoinGame(string id)
        {
            var playerIdStr = Request.Cookies["PlayerId"];
            if (string.IsNullOrEmpty(playerIdStr) || !int.TryParse(playerIdStr, out int playerId))
            {
                return RedirectToAction("Index", "Home");
            }
            
            var game = _context.GameSessions.Include(g => g.Player2).FirstOrDefault(g => g.Id == id);
            if (game != null && game.Status == "Waiting" && game.Player1Id != playerId)
            {
                game.Player2Id = playerId;
                game.Status = "InProgress";
                
                // Randomly decide who goes first
                game.CurrentTurnPlayerId = new Random().Next(2) == 0 ? game.Player1Id : game.Player2Id;
                
                await _context.SaveChangesAsync();
                
                var player2 = _context.Players.Find(playerId);
                await _hubContext.Clients.Group(id).SendAsync("GameStarted", player2.Name, game.CurrentTurnPlayerId);
            }
            
            return RedirectToAction("Game", new { id = id });
        }
        
        public IActionResult Game(string id)
        {
            var playerIdStr = Request.Cookies["PlayerId"];
            if (string.IsNullOrEmpty(playerIdStr) || !int.TryParse(playerIdStr, out int playerId))
            {
                return RedirectToAction("Index", "Home");
            }
            
            var game = _context.GameSessions.Include(g => g.Player1).Include(g => g.Player2).FirstOrDefault(g => g.Id == id);
            if (game == null) return RedirectToAction("Index");
            
            ViewBag.PlayerId = playerId;
            return View(game);
        }
    }
}
