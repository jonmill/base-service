using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Warbreaker.Configuration;

/// <summary>
/// Extension class for providing configuration data
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds the correct configuration data to the builder
    /// </summary>
    public static IConfigurationBuilder AddConfigurationSettings(this IConfigurationBuilder configurationBuilder, string environmentName, string[] args)
    {
        configurationBuilder.AddCommandLine(args)
                            .AddEnvironmentVariables();

        // Add secret info for the correct environment
        if (string.Equals(environmentName, Constants.DEVELOPMENT_ENVIRONMENT_NAME, StringComparison.OrdinalIgnoreCase))
        {
            // Add the user secret ID from the csproj file
            configurationBuilder.AddUserSecrets(Constants.USER_SECRET_ID);
        }
        else
        {
            // For non-dev environments, get the secrets from key vault
            string? vaultUrl = Environment.GetEnvironmentVariable(Constants.KEY_VAULT_URL_ENVIRONMENT_VARIABLE);
            if (string.IsNullOrEmpty(vaultUrl))
            {
                throw new InvalidOperationException($"Could not find KeyVault URL as Environment Variable for EnvVar Name `{Constants.KEY_VAULT_URL_ENVIRONMENT_VARIABLE}`");
            }

            // This depends on using Azure Managed Identity in the running service
            configurationBuilder.AddAzureKeyVault(new Uri(vaultUrl), new DefaultAzureCredential());
        }

        return configurationBuilder;
    }

    /// <summary>
    /// Retrieves the paramters needed to validate a JWT token
    /// </summary>
    public static TokenValidationParameters GetJwtValidationParameters(this IConfiguration configuration)
    {
        SymmetricSecurityKey key = new SymmetricSecurityKey(GetJwtTokenSecret(configuration));

        return new TokenValidationParameters()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateActor = true,
            ValidateLifetime = true,
            IssuerSigningKey = key,
        };
    }

    /// <summary>
    /// Generates a JWT token with the given expiry
    /// </summary>
    public static JwtSecurityToken GenerateJwtToken(this IConfiguration configuration, TimeSpan expiresIn, IEnumerable<Claim> claims)
    {
        SymmetricSecurityKey key = new SymmetricSecurityKey(GetJwtTokenSecret(configuration));
        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        DateTimeOffset validUntil = DateTimeOffset.Now.Add(expiresIn);
        return new JwtSecurityToken(
            Constants.JWT_ISSUER,
            Constants.JWT_AUDIENCE,
            claims,
            DateTimeOffset.UtcNow.DateTime,
            validUntil.DateTime,
            creds);
    }

    /// <summary>
    /// Retrieves the JWT signing secret
    /// </summary>
    private static byte[] GetJwtTokenSecret(this IConfiguration configuration)
    {
        string? jwtSecret = configuration.GetValue<string>(Constants.JWT_SIGNING_KEY_CONFIG_NAME);
        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new InvalidOperationException($"Could not find JWT Signing Key at `{Constants.JWT_SIGNING_KEY_CONFIG_NAME}`");
        }

        return Convert.FromBase64String(jwtSecret);
    }

    /// <summary>
    /// Retrieves the Database Connection String
    /// </summary>
    public static string GetDatabaseConnectionString(this IConfiguration configuration)
    {
        string? connectionString = configuration.GetValue<string>(Constants.DB_CONNECTION_STRING);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Could not find Database Connection String at `{Constants.DB_CONNECTION_STRING}`");
        }

        return connectionString;
    }
}
