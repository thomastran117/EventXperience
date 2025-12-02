using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using System.Data.Common;
using backend.Resources;

namespace backend.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly AppDatabaseContext _context;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        protected BaseRepository(AppDatabaseContext context)
        {
            _context = context;

            _retryPolicy = Policy
                .Handle<Exception>(IsTransient)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                    onRetry: (ex, delay, attempt, ctx) =>
                    {
                    });

            _circuitBreakerPolicy = Policy
                .Handle<Exception>(IsTransient)
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromSeconds(10),
                    onBreak: (ex, delay) =>
                    {
                       
                    },
                    onReset: () =>
                    {
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
                        {
                            throw;
                        }
                        throw;
                    }
                });
        }
    }
}
