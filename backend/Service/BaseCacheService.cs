using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace backend.Services
{
    public abstract class BaseCacheService
    {
        protected readonly IDatabase _db;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        protected BaseCacheService(IDatabase db)
        {
            _db = db;

            _retryPolicy = Policy
                .Handle<Exception>(IsTransientRedisError)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromMilliseconds(50 * attempt),
                    onRetry: (ex, delay, attempt, ctx) =>
                    {
                    });

            _circuitBreakerPolicy = Policy
                .Handle<Exception>(IsTransientRedisError)
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(5),
                    onBreak: (ex, delay) =>
                    {
                    },
                    onReset: () =>
                    {
                    });
        }

        protected static bool IsTransientRedisError(Exception ex)
        {
            return ex is RedisConnectionException ||
                   ex is RedisTimeoutException ||
                   ex is SocketException ||
                   ex is TimeoutException;
        }

        protected async Task<T> ExecuteAsync<T>(Func<Task<T>> action, T fallback = default!)
        {
            try
            {
                return await _retryPolicy
                    .WrapAsync(_circuitBreakerPolicy)
                    .ExecuteAsync(async () => await action());
            }
            catch (Exception ex)
            {
                return fallback;
            }
        }
    }
}
