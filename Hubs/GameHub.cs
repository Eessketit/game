using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using TicTacToeApp.Data;

namespace TicTacToeApp.Hubs
{
    public class GameHub : Hub
    {
        private readonly AppDbContext _context;

        public GameHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task JoinGame(string gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        }

        public async Task LeaveGame(string gameId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
        }

        public async Task MakeMove(string gameId, int playerId, int position)
        {
            var game = await _context.GameSessions.FindAsync(gameId);
            if (game == null || game.Status != "InProgress" || game.CurrentTurnPlayerId != playerId)
            {
                return;
            }

            if (game.BoardState[position] != '-') return;

            char marker = game.Player1Id == playerId ? 'X' : 'O';
            
            char[] board = game.BoardState.ToCharArray();
            board[position] = marker;
            game.BoardState = new string(board);

            string winner = CheckWinner(game.BoardState);
            if (winner != null)
            {
                game.Status = "Finished";
                if (winner == "X" || winner == "O")
                {
                    int winnerId = winner == "X" ? game.Player1Id.Value : game.Player2Id.Value;
                    game.WinnerId = winnerId;
                    
                    var p1 = await _context.Players.FindAsync(game.Player1Id);
                    var p2 = await _context.Players.FindAsync(game.Player2Id);
                    if (winnerId == game.Player1Id) { p1.Wins++; p2.Losses++; }
                    else { p2.Wins++; p1.Losses++; }
                }
                else if (winner == "Draw")
                {
                    var p1 = await _context.Players.FindAsync(game.Player1Id);
                    var p2 = await _context.Players.FindAsync(game.Player2Id);
                    p1.Draws++; p2.Draws++;
                }
            }
            else
            {
                game.CurrentTurnPlayerId = game.Player1Id == playerId ? game.Player2Id : game.Player1Id;
            }

            await _context.SaveChangesAsync();
            await Clients.Group(gameId).SendAsync("ReceiveMove", position, marker.ToString(), game.CurrentTurnPlayerId, game.Status, winner);
        }

        private string CheckWinner(string b)
        {
            if (b[0] != '-' && b[0] == b[1] && b[1] == b[2]) return b[0].ToString();
            if (b[3] != '-' && b[3] == b[4] && b[4] == b[5]) return b[3].ToString();
            if (b[6] != '-' && b[6] == b[7] && b[7] == b[8]) return b[6].ToString();
            
            if (b[0] != '-' && b[0] == b[3] && b[3] == b[6]) return b[0].ToString();
            if (b[1] != '-' && b[1] == b[4] && b[4] == b[7]) return b[1].ToString();
            if (b[2] != '-' && b[2] == b[5] && b[5] == b[8]) return b[2].ToString();
            
            if (b[0] != '-' && b[0] == b[4] && b[4] == b[8]) return b[0].ToString();
            if (b[2] != '-' && b[2] == b[4] && b[4] == b[6]) return b[2].ToString();
            
            if (!b.Contains("-")) return "Draw";
            
            return null;
        }
    }
}
