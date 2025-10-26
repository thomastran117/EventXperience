using DotNetEnv;
using backend.Utilities;

namespace backend.Config
{
    public static class EnvManager
    {
        static EnvManager()
        {
            try
            {
                Env.Load();
                Logger.Info(".env file loaded successfully.");
            }
            catch
            {
                Logger.Warn("No .env file found — relying on system environment variables.");
            }

            try
            {
                ValidateRequiredVariables();
            }
            catch (Exception ex)
            {
                Logger.Error($"Environment validation failed: {ex.Message}");
                Environment.Exit(1);
            }
        }

        public static string DbConnectionString => GetRequired("DB_CONNECTION_STRING");
        public static string RedisConnection => GetRequired("REDIS_CONNECTION");

        public static string JwtIssuer => GetRequired("JWT_ISSUER");
        public static string JwtAudience => GetRequired("JWT_AUDIENCE");
        public static string JwtSecretKey => GetRequired("JWT_SECRET_KEY");

        public static string JwtIssuerRefresh => GetRequired("JWT_ISSUER_REFRESH");
        public static string JwtAudienceRefresh => GetRequired("JWT_AUDIENCE_REFRESH");
        public static string JwtSecretKeyRefresh => GetRequired("JWT_SECRET_KEY_REFRESH");
        public static string? Email => GetOptional("EMAIL");
        public static string? Password => GetOptional("EMAIL_PASSWORD");
        public static string? SmtpServer => GetOptional("SMTP_SERVER");
        public static string AppEnvironment => GetOptional("APP_ENV") ?? "development";
        public static string LogLevel => GetOptional("LOG_LEVEL") ?? "info";
        private static string GetRequired(string key)
        {
            var val = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(val))
                throw new Exception($"Missing required environment variable: {key}");
            return val;
        }

        private static string? GetOptional(string key)
        {
            var val = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(val))
            {
                Logger.Debug($"Optional variable {key} not set.");
                return null;
            }
            return val;
        }
        private static void ValidateRequiredVariables()
        {
            var requiredKeys = new[]
            {
                "DB_CONNECTION_STRING",
                "REDIS_CONNECTION",
                "JWT_ISSUER",
                "JWT_AUDIENCE",
                "JWT_SECRET_KEY",
                "JWT_ISSUER_REFRESH",
                "JWT_AUDIENCE_REFRESH",
                "JWT_SECRET_KEY_REFRESH"
            };

            var missing = requiredKeys
                .Where(k => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(k)))
                .ToList();

            if (missing.Any())
                throw new Exception($"Missing required variables: {string.Join(", ", missing)}");
        }
    }
}
