using Azure.Identity;
using Microsoft.Extensions.Configuration;

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
                throw new InvalidOperationException($"Did not find KeyVault URL as Environment Variable for EnvVar Name `{Constants.KEY_VAULT_URL_ENVIRONMENT_VARIABLE}`");
            }

            // This depends on using Azure Managed Identity in the running service
            configurationBuilder.AddAzureKeyVault(new Uri(vaultUrl), new DefaultAzureCredential());
        }

        return configurationBuilder;
    }
}
