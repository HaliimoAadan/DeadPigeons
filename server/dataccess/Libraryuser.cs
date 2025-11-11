namespace dataccess;

public class Libraryuser
{
    public string Id { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public string Salt { get; set; } = null!;

    public string Role { get; set; } = null!;
}