using api.DTOs.Requests;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api;

public class AuthController(IAuthService authService) : ControllerBase
{

    
    [HttpPost(nameof(Login))]
    public async Task<JwtResponse> Login([FromBody] LoginRequestDto dto)
    {
        return await authService.Login(dto);
    }
    
    [HttpPost(nameof(Register))]
    public async Task<JwtResponse> Register([FromBody] RegisterRequestDto dto)
    {
        return await authService.Register(dto);
    }
    [HttpPost(nameof(WhoAmI))]
    public async Task<JwtClaims> WhoAmI([FromHeader]string authorization)
    {
        return await authService.VerifyAndDecodeToken(authorization);
    }
    
    
}