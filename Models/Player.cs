using System.ComponentModel.DataAnnotations;

namespace TicTacToeApp.Models
{
    public class Player
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
    }
}
