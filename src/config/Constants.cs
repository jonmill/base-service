namespace Warbreaker.Configuration;

internal static class Constants
{
    internal const string KEY_VAULT_URL_ENVIRONMENT_VARIABLE = "VaultUrl";
    internal const string USER_SECRET_ID = "3a7baa6c-2336-4fa2-a35e-1f3b59c1604b"; // NOTE - if this changes, make sure it matches the CSPROJ entry
    internal const string DEVELOPMENT_ENVIRONMENT_NAME = "development";
    internal const string APP_INSIGHTS_KEY_NAME = "LoggingApiKey";
    internal const string JWT_SIGNING_KEY_CONFIG_NAME = "JwtSecret";
    internal const string JWT_ISSUER = "Warbreaker";
    internal const string JWT_AUDIENCE = "Warbreaker_User_Audience";
    internal const string DB_CONNECTION_STRING = "DbConnectionString";
}
