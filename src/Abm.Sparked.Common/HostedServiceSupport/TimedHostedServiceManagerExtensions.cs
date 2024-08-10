using Abm.Pyro.Application.HostedServiceSupport;
using Microsoft.Extensions.DependencyInjection;

namespace Abm.Sparked.Common.HostedServiceSupport;

public static class TimedHostedServiceManagerExtensions
{
    public static void AddTimedHostedService<T>(this IServiceCollection services,
        Action<TimedHostedServiceManagerOptions<T>> configurator) where T : class, ITimedHostedService
    {
        var options = new TimedHostedServiceManagerOptions<T>();
        configurator(options);
        services.AddSingleton(x => options);
        services.AddScoped<T>();
        services.AddHostedService<TimedHostedServiceManager<T>>();
    }
}