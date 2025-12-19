using api.Etc;
using efscaffold;

namespace tests;

public class SetupTests(MyDbContext ctx,
    ISeeder seeder,
    ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task SeederDoesNotThrowException()
    {
        await seeder.Seed();
    }
}