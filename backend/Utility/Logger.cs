namespace backend.Utilities
{
    public static class Logger
    {
        private static object _lock = new();

        private static void Write(string level, ConsoleColor color, string message)
        {
            lock (_lock)
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var originalColor = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"[{timestamp}] ");

                Console.ForegroundColor = color;
                Console.Write($"[{level}] ");

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);

                Console.ForegroundColor = originalColor;
            }
        }

        public static void Info(string message)  => Write("INFO ", ConsoleColor.Cyan, message);
        public static void Debug(string message) => Write("DEBUG", ConsoleColor.Gray, message);
        public static void Warn(string message)  => Write("WARN ", ConsoleColor.Yellow, message);
        public static void Error(string message) => Write("ERROR", ConsoleColor.Red, message);
    }
}
