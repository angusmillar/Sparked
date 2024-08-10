using ConsoleApp;
using FhirNavigator;
using Abm.Sparked.Common.Validator;
using Abm.Sparked.eRequesting.Demo.Placer.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Error)
    .WriteTo.Console()
    .WriteTo.File("application.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();
    
IServiceCollection services = new ServiceCollection();

services.AddLogging();


//Configuration
services.AddOptions<ApplicationConfiguration>().Bind(configuration.GetSection(ApplicationConfiguration.SectionName));

//Add Services
services.AddScoped<Application>();
services.AddScoped<IServiceRequestValidator, ServiceRequestValidator>();
services.AddScoped<IPatientValidator, PatientValidator>();

FhirNavigatorSettings? fhirNavigatorSettings = configuration.GetRequiredSection(FhirNavigatorSettings.SectionName)
    .Get<FhirNavigatorSettings>();
ArgumentNullException.ThrowIfNull(fhirNavigatorSettings);
services.AddFhirNavigator(settings =>
{
    settings.FhirRepositories = fhirNavigatorSettings.FhirRepositories;
    settings.Proxy = fhirNavigatorSettings.Proxy;
});

var serviceProvider = services.BuildServiceProvider();

serviceProvider.GetRequiredService<ILoggerFactory>()
    .AddSerilog();


var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
await using var scope = serviceScopeFactory.CreateAsyncScope();
await scope.ServiceProvider.GetRequiredService<Application>().Run();