using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Warbreaker.Configuration;
using Warbreaker.Data;
using Warbreaker.Services;

const string JWT_SIGNING_KEY_CONFIG_NAME = "JwtSecret";
const string DB_CONNECTION_STRING = "DbConnectionString";

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    serverOptions.Listen(System.Net.IPAddress.Loopback, 7224, opts =>
    {
        opts.UseHttps();
    });
});

builder.Services.AddGrpc(opts =>
{
    opts.EnableDetailedErrors = true;
});

// Add configurations and logging
builder.Configuration.AddConfigurationSettings(builder.Environment.EnvironmentName, args);
builder.Logging.AddAppLogging(builder.Configuration, builder.Environment.EnvironmentName);

// Add authentication
string? jwtSigningKeyString = builder.Configuration.GetValue<string>(JWT_SIGNING_KEY_CONFIG_NAME);
if (string.IsNullOrEmpty(jwtSigningKeyString))
{
    throw new InvalidOperationException($"Could not find JWT Signing Key at `{JWT_SIGNING_KEY_CONFIG_NAME}`");
}

#warning Switch to an asymmetric key in the future, for better security. This will work for now.
byte[] jwtSigningKey = Convert.FromBase64String(jwtSigningKeyString);
Microsoft.IdentityModel.Tokens.SymmetricSecurityKey securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(jwtSigningKey);

// Add JWT authentication and authorizations
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateActor = true,
            ValidateLifetime = true,
            IssuerSigningKey = securityKey,
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(ClaimTypes.Actor);
        policy.RequireClaim(ClaimTypes.Role);
    });
});

// Add database provider
builder.Services.AddDbContext<DatabaseProvider>(options =>
{
    string? dbConnection = builder.Configuration.GetValue<string>(DB_CONNECTION_STRING);
    if (string.IsNullOrEmpty(dbConnection))
    {
        throw new InvalidOperationException($"Could not find Database Connection String at `{DB_CONNECTION_STRING}`");
    }

    options.UseSqlite(dbConnection, sqliteOptions =>
    {
        sqliteOptions.CommandTimeout(5); // Timeout after 5 seconds
    });
});


WebApplication app = builder.Build();

app.UseRouting();
app.UseGrpcWeb(new GrpcWebOptions() { DefaultEnabled = true });

// Add authenticate then authorize before adding endpoints
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(eps =>
{
    eps.MapGrpcService<GreeterService>().EnableGrpcWeb();
});

app.Run();
