using System.Net.Sockets;

using backend.Resources;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

using StackExchange.Redis;

namespace backend.Services
{
    public abstract class BaseCacheService
    {
        protected readonly IDatabase _db;
        protected readonly IConnectionMultiplexer _redis;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        protected BaseCacheService(RedisResource redisResource)
        {
            _db = redisResource.Database;
            _redis = redisResource.Multiplexer;

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
