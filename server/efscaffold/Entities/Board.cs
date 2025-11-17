using System;
using System.Collections.Generic;

namespace efscaffold.Entities;

public partial class Board
{
    public Guid BoardId { get; set; }

    public Guid PlayerId { get; set; }

    public Guid GameId { get; set; }

    public List<int> ChosenNumbers { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsRepeating { get; set; }

    public Guid? RepeatUntilGameId { get; set; }

    public DateTime Timestamp { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual Player Player { get; set; } = null!;

    public virtual Game? RepeatUntilGame { get; set; }

    public virtual ICollection<Winningboard> Winningboards { get; set; } = new List<Winningboard>();
}
