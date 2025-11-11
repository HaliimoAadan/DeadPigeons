namespace dataccess;

public class Genre
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}