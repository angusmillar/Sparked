using FhirNavigator;
using Abm.Sparked.Common.Validator;
using Abm.Sparked.eRequesting.Demo.Common.HostedServiceSupport;
using Abm.Sparked.eRequesting.Demo.Common.Managers;
using Abm.Sparked.eRequesting.Demo.Common.Services;
using Abm.Sparked.eRequesting.Demo.Common.Settings;
using Abm.Sparked.eRequesting.Demo.Common.ViewModels;
using MudBlazor.Services;
using Abm.Sparked.eRequesting.Demo.WebApp.Components;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
   
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger);

//builder.Services.AddLogging();

builder.Services.AddOptions<WebAppSettings>()
    .Bind(builder.Configuration.GetSection(WebAppSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

//FHIR Navigator Service
FhirNavigatorSettings fhirNavigatorSettings = builder.Configuration.GetRequiredSection(FhirNavigatorSettings.SectionName)
    .Get<FhirNavigatorSettings>() ?? throw new NullReferenceException($"No {FhirNavigatorSettings.SectionName} settings found!");

builder.Services.AddFhirNavigator(settings =>
{
    settings.FhirRepositories = fhirNavigatorSettings.FhirRepositories;
    settings.Proxy = fhirNavigatorSettings.Proxy;
});

builder.Services.AddScoped<IRequestingService, ServiceRequestingService>();
builder.Services.AddScoped<IServiceRequestValidator, ServiceRequestValidator>();
builder.Services.AddScoped<IPatientValidator, PatientValidator>();
builder.Services.AddScoped<ITaskValidator, TaskValidator>();
builder.Services.AddScoped<IPractitionerRoleValidator, PractitionerRoleValidator>();
builder.Services.AddScoped<IOrganizationRoleValidator, OrganizationRoleValidator>();

//builder.Services.AddScoped<FhirTaskManager>();
builder.Services.AddTimedHostedService<FhirTaskManager>(options =>
    {
      options.TriggersEvery = TimeSpan.FromSeconds(5);
   });

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Abm.Sparked.eRequesting.Demo.WebApp.Client._Imports).Assembly);


app.MapGet("/serviceRequestVms", async (IRequestingService requestingService) =>
{
    return await requestingService.GetServiceRequestVmList();
});

app.MapGet("/serviceRequestJson/{id}", async (string id, IRequestingService requestingService) =>
{
    return await requestingService.GetServiceRequestJson(resourceId: id);
});

app.MapGet("/taskVms", async (IRequestingService requestingService) =>
{
    return await requestingService.GetTaskVmList();
});

app.Run();