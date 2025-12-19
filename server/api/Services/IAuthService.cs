using api.Models;
using api.Models.Requests;
using efscaffold.Entities;

namespace api.Services;

public interface IAuthService
   {
       Task<JwtClaims> VerifyAndDecodeToken(string? token);
       Task<JwtResponse> Login(LoginRequestDto dto);
       Task<JwtResponse> RegisterPlayer(RegisterRequestDto dto);
       Task<JwtResponse> RegisterAdmin(Admin admin);
   }
   