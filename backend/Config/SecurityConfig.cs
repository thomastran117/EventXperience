namespace backend.Configs;

public static class CorsExtensions
{
    public static IServiceCollection AddReactCors(this IServiceCollection services, string policyName = "AllowReact")
    {
        services.AddCors(options =>
        {
            options.AddPolicy(policyName, policy =>
            {
                policy.WithOrigins("http://localhost:3030")
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        return services;
    }
}