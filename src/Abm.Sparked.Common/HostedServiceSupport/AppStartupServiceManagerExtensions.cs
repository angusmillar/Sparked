using Abm.Pyro.Application.HostedServiceSupport;
using Microsoft.Extensions.DependencyInjection;

namespace Abm.Sparked.Common.HostedServiceSupport;

public static class   AppStartupServiceManagerExtensions
{
    /// <summary>
    /// Runs a blocking service on application startup before the request pipeline is active.
    /// The service must implement the IRunAppStartupService interface  
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="T"></typeparam>
    public static void AddAppStartUpService<T>(this IServiceCollection services) where T : class, IAppStartupService
    {
        services.AddScoped<T>();
        services.AddHostedService<AppStartupServiceManager<T>>();
    }
}