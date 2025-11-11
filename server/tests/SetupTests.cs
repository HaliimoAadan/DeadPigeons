using api.DTOs.Requests;
using api.Services;
using dataccess;

namespace tests;

public class SetupTests(MyDbContext ctx, IAuthService authService)
{
    [Fact]
    public void XunitDependencyInjectionCanFindService()
    {
        Assert.Equal(0, ctx.Authors.ToList().Count);
    }

    [Fact]
    public async Task TimeProviderTimestamptsAsJan1_2000()
    {
        var result = await authService.Register(new RegisterRequestDto
        {
            Email = "tes@email.dk",
            Password = "asædkjlsadjsadjlksad"
        });
    }
}