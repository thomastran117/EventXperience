namespace backend.Config
{
    public static class SecurityCOnfig
    {
        public static IServiceCollection AddReactCors(this IServiceCollection services, string policyName = "AllowReact")
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, policy =>
                {
                    policy.WithOrigins("http://localhost:3090")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            return services;
        }
    }
}
