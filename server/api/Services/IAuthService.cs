using api.DTOs.Requests;

namespace api.Services;

public interface IAuthService
{
    Task<JwtClaims> VerifyAndDecodeToken(string? token);

    Task<JwtResponse> Login(LoginRequestDto dto);
    Task<JwtResponse> Register(RegisterRequestDto dto);
}