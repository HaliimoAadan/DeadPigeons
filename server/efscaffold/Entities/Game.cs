using System;
using System.Collections.Generic;

namespace efscaffold.Entities;

public partial class Game
{
    public Guid GameId { get; set; }

    public List<int>? WinningNumbers { get; set; }

    public DateTime? DrawDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    public virtual ICollection<Board> BoardGames { get; set; } = new List<Board>();

    public virtual ICollection<Board> BoardRepeatUntilGames { get; set; } = new List<Board>();

    public virtual ICollection<Winningboard> Winningboards { get; set; } = new List<Winningboard>();
}
