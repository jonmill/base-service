using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Warbreaker.Configuration;
using Warbreaker.Data;
using Warbreaker.Services;

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

// Add JWT authentication and authorizations
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = builder.Configuration.GetJwtValidationParameters();
    })
    .Services.AddAuthorization(options =>
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
    string conString = builder.Configuration.GetDatabaseConnectionString();
    options.UseSqlite(conString, sqliteOptions =>
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
    eps.MapGrpcService<LoginService>().EnableGrpcWeb();
});

app.Run();
