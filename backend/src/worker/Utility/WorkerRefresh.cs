namespace worker.Utilities
{
    public static class CacheRefreshPolicy
    {
        public static bool ShouldRefresh(
            TimeSpan? ttl,
            TimeSpan refreshAhead)
        {
            if (!ttl.HasValue)
                return true;

            if (ttl.Value <= TimeSpan.Zero)
                return true;

            return ttl.Value <= refreshAhead;
        }
    }
}
