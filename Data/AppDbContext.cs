using Microsoft.EntityFrameworkCore;
using TicTacToeApp.Models;

namespace TicTacToeApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
    }
}
