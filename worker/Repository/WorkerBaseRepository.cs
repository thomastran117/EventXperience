using System.Data.Common;

using Microsoft.EntityFrameworkCore;

using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

using worker.Resources;

namespace worker.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly WorkerDatabaseContext _context;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private static readonly Random Jitterer = new Random();

        protected BaseRepository(WorkerDatabaseContext context)
        {
            _context = context;

            _retryPolicy = Policy
                .Handle<Exception>(IsTransient)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt =>
                    {
                        double baseDelayMs = 100 * Math.Pow(2, attempt);
                        double jitterFactor = 0.5 + Jitterer.NextDouble();

                        return TimeSpan.FromMilliseconds(baseDelayMs * jitterFactor);
                    },
                    onRetry: (ex, delay, attempt, ctx) =>
                    {
                        // logger
                    });

            _circuitBreakerPolicy = Policy
                .Handle<Exception>(IsTransient)
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromSeconds(10),
                    onBreak: (ex, delay) =>
                    {
                        // logging
                    },
                    onReset: () =>
                    {
                        // logging
                    });
        }

        protected static bool IsTransient(Exception ex)
        {
            if (ex is TimeoutException)
                return true;

            if (ex is DbException)
                return true;

            if (ex is DbUpdateException dbUpdateEx &&
                dbUpdateEx.InnerException is DbException)
                return true;

            return false;
        }

        protected async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            return await _retryPolicy
                .WrapAsync(_circuitBreakerPolicy)
                .ExecuteAsync(async () =>
                {
                    try
                    {
                        return await action();
                    }
                    catch (Exception ex)
                    {
                        if (!IsTransient(ex))
                            throw;

                        throw;
                    }
                });
        }
    }
}
