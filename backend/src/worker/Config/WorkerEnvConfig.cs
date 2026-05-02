using DotNetEnv;

using worker.Utilities;

namespace worker.Config
{
    public static class EnvManager
    {
        private static readonly bool _runningInContainer;
        private static readonly string _dbConnectionString;
        private static readonly string _redisConnection;
        private static readonly string _rabbitConnection;
        private static readonly string? _email;
        private static readonly string? _password;
        private static readonly string? _smtpServer;
        private static readonly string _frontendUrl;
        private static readonly string _appEnvironment;
        private static readonly string _logLevel;

        static EnvManager()
        {
            _runningInContainer = IsRunningInContainer();
            TryLoadEnvFile();

            _dbConnectionString = GetOrDefault(
                ["DB_CONNECTION_STRING"],
                "Server=localhost;Port=3306;Database=database;User=root;Password=password123"
            );

            _redisConnection = GetOrDefault(
                ["REDIS_CONNECTION", "REDIS_URL"],
                "localhost:6379"
            );

            _rabbitConnection = GetOrDefault(
                ["RABBIT_CONNECTION", "RABBITMQ_URL"],
                "amqp://guest:guest@localhost:5672"
            );

            _email = GetOptional(["EMAIL", "EMAIL_USER"]);
            _password = GetOptional(["EMAIL_PASSWORD"]);
            _smtpServer = GetOptional(["SMTP_SERVER"]);
            _frontendUrl = GetOrDefault(["FRONTEND_URL"], "http://localhost:3090");

            _appEnvironment = GetOrDefault(
                ["APP_ENV", "DOTNET_ENVIRONMENT", "ASPNETCORE_ENVIRONMENT"],
                "development"
            ).ToLowerInvariant();
            _logLevel = GetOrDefault(["LOG_LEVEL"], "info").ToLowerInvariant();
        }

        private static void TryLoadEnvFile()
        {
            if (_runningInContainer)
            {
                Logger.Info(
                    "Running in container; skipping .env file discovery and using injected environment variables."
                );
                return;
            }

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = new DirectoryInfo(baseDir);

            while (dir != null)
            {
                var envPath = Path.Combine(dir.FullName, ".env");

                if (File.Exists(envPath))
                {
                    try
                    {
                        Env.Load(envPath);
                        Logger.Info($".env file loaded from: {envPath}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, $"Failed to load .env file at {envPath}");
                    }
                    return;
                }

                dir = dir.Parent;
            }

            Logger.Debug("No .env file found in directory hierarchy; using system environment variables.");
        }

        private static string? GetOptional(params string[] keys)
        {
            foreach (var key in keys)
            {
                var val = Environment.GetEnvironmentVariable(key);

                if (!string.IsNullOrWhiteSpace(val))
                    return val;
            }

            Logger.Debug(
                $"Optional environment variable(s) not set: {string.Join(", ", keys)}."
            );
            return null;
        }

        private static string GetOrDefault(string[] keys, string fallback) =>
            GetOptional(keys) ?? fallback;

        private static bool IsRunningInContainer()
        {
            return bool.TryParse(
                    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                    out var running
                ) && running;
        }

        public static string DbConnectionString => _dbConnectionString;
        public static string RedisConnection => _redisConnection;
        public static string RabbitConnection => _rabbitConnection;
        public static string? Email => _email;
        public static string? Password => _password;
        public static string? SmtpServer => _smtpServer;
        public static string FrontendUrl => _frontendUrl;
        public static string AppEnvironment => _appEnvironment;
        public static string LogLevel => _logLevel;

        public static void Validate()
        {
            if (_appEnvironment is "development" or "test")
            {
                Logger.Warn("Skipping environment validation (dev/test mode).");
                return;
            }

            var required = new Dictionary<string, string>
            {
                { "DB_CONNECTION_STRING", _dbConnectionString },
                { "REDIS_CONNECTION", _redisConnection },
            };

            var missing = required
                .Where(kv => string.IsNullOrWhiteSpace(kv.Value))
                .Select(kv => kv.Key)
                .ToList();

            if (missing.Any())
                throw new InvalidOperationException(
                    $"Missing required environment variables: {string.Join(", ", missing)}"
                );

            Logger.Info("Environment variables validated successfully.");
        }
    }
}
