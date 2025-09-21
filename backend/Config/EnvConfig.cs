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
            }
            catch
            {
                Logger.Error("No .env file found, server startup aborting");
                Environment.Exit(1);
            }
        }

        public static string DbConnectionString
        {
            get
            {
                var val = Get("DB_CONNECTION_STRING");
                if (string.IsNullOrEmpty(val))
                {
                    Logger.Warn("DB_CONNECTION_STRING not set. Using default localhost connection.");
                    return "Host=localhost;Port=5432;Database=schoolapp;Username=postgres;Password=postgres";
                }
                return val;
            }
        }

        public static string RedisConnection
        {
            get
            {
                var val = Get("REDIS_CONNECTION");
                if (string.IsNullOrEmpty(val))
                {
                    Logger.Warn("REDIS_CONNECTION not set. Using default localhost connection.");
                    return "localhost:6379";
                }
                return val;
            }
        }

        public static string JwtIssuer =>
            GetRequired("Jwt:Issuer");

        public static string JwtAudience =>
            GetRequired("Jwt:Audience");

        public static string JwtSecretKey =>
            GetRequired("Jwt:SecretKey");

        private static string? Get(string key) =>
            Environment.GetEnvironmentVariable(key);

        private static string GetRequired(string key)
        {
            var val = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(val))
                throw new Exception($"Missing required environment variable: {key}");
            return val;
        }
    }
}
