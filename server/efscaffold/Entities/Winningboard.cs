using System;
using System.Collections.Generic;

namespace efscaffold.Entities;

public partial class Winningboard
{
    public Guid WinningboardId { get; set; }

    public Guid GameId { get; set; }

    public Guid BoardId { get; set; }

    public int WinningNumbersMatched { get; set; }

    public DateTime Timestamp { get; set; }

    public virtual Board Board { get; set; } = null!;

    public virtual Game Game { get; set; } = null!;
}
