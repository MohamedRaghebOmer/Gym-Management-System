using System.Threading.RateLimiting;

namespace GYM.API.Extensions;

internal static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter =
                PartitionedRateLimiter.Create<HttpContext, string>(
                    _ => RateLimitPartition.GetSlidingWindowLimiter(
                        "global",
                        _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            QueueLimit = 0
                        }));

        });

        return services;
    }
}