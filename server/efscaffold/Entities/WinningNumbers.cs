namespace efscaffold.Entities;

public class WinningNumbers
{
        public Guid WinningNumbersId { get; set; }
        public Guid GameId { get; set; }
        public Game Game { get; set; } = null!;
        public int[] Numbers { get; set; } = null!; 
}