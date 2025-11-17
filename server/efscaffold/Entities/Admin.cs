using System;
using System.Collections.Generic;

namespace efscaffold.Entities;

public partial class Admin
{
    public Guid AdminId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
}
