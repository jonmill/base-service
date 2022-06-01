using Grpc.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Warbreaker.Configuration;
using Warbreaker.Protobufs;

namespace Warbreaker.Services;

public class LoginService : Login.LoginBase
{
    private readonly IConfiguration _config;
    private readonly JwtSecurityTokenHandler _jwtHandler;
    private readonly ILogger<LoginService> _logger;

    public LoginService(IConfiguration configuration, ILogger<LoginService> logger)
    {
        _config = configuration;
        _jwtHandler = new JwtSecurityTokenHandler();
        _logger = logger;
    }

    public override Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        JwtSecurityToken token = _config.GenerateJwtToken(TimeSpan.FromDays(7), new List<Claim>()
        {
            new Claim(ClaimTypes.Actor, "1"),
            new Claim("user_name", "Jon"),
        });

        return Task.FromResult(new LoginResponse()
        {
            JwtToken = _jwtHandler.WriteToken(token),
        });
    }
}
