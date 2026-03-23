using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToeApp.Models
{
    public class GameSession
    {
        [Key]
        [StringLength(36)]
        public string Id { get; set; }

        public string Status { get; set; } = "Waiting"; // Waiting, InProgress, Finished

        public int? Player1Id { get; set; }
        [ForeignKey("Player1Id")]
        public Player Player1 { get; set; }

        public int? Player2Id { get; set; }
        [ForeignKey("Player2Id")]
        public Player Player2 { get; set; }

        // Store board as a 9-character string. 'X', 'O', or '-'
        public string BoardState { get; set; } = "---------"; 

        public int? CurrentTurnPlayerId { get; set; }
        
        public int? WinnerId { get; set; }
    }
}
