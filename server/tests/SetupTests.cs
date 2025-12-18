/*using System.Text.Json;
using api.Etc;
using api.Models.Requests;
using api.Services;
using Infrastructure.Postgres.Scaffolding;

namespace tests;

public class SetupTests(MyDbContext ctx,
    ISeeder seeder,
    ITestOutputHelper outputHelper)
{

    [Fact]
    public async Task RegisterReturnsJwtWhichCanVerifyAgain()
    {
        var result = await authService.Register(new RegisterRequestDto
        {
            Email = "tes@email.dk",
            Password = "asædkjlsadjsadjlksad"
        });
        outputHelper.WriteLine(result.Token);
        var token = await authService.VerifyAndDecodeToken(result.Token); //Does not throw is the "assertion" here
        outputHelper.WriteLine(JsonSerializer.Serialize(token));
    }

    [Fact]
    public async Task SeederDoesNotThrowException()
    {
        await seeder.Seed();
    }
}*/