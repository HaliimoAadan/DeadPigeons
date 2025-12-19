using efscaffold;

namespace api.Etc;

public interface ISeeder
{
    public Task Seed();
}

public class Seeder(MyDbContext ctx) : ISeeder
{
    public async Task Seed()
    {
        ctx.Database.EnsureCreated();
    }
}