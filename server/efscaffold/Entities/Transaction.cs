using System;
using System.Collections.Generic;

namespace efscaffold.Entities;

public partial class Transaction
{
    public Guid TransactionId { get; set; }

    public Guid PlayerId { get; set; }

    public string MobilepayReqId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public virtual Player Player { get; set; } = null!;
}
