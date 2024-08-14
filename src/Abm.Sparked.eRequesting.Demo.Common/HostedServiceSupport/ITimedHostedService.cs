namespace Abm.Sparked.eRequesting.Demo.Common.HostedServiceSupport;

/// <summary>
/// This interface can be implemented and run as a background hosted service using the service extension services.AddTimedHostedService&amp;lt;T&amp;gt;()
/// which allows you to configure how often the service is run and how long before it first runs.
/// If the background hosted service takes longer to run than its given cycle time 'TimerThenFiresEvery'. 
/// Then that execution is skipped until the next timer cycle fires, where it will check for completion again.
/// Below is an example service registration where 'MyTimedHostedService' implements this 'ITimedHostedService' interface:
/// 
/// builder.Services.AddTimedHostedService&lt;FhirTaskManager&gt;(options =>
/// {
///     options.TriggersEvery = TimeSpan.FromSeconds(3);
/// });
/// You can also repeat the above registrations to run many different services on different timers.
/// </summary>
public interface ITimedHostedService
{
    public Task DoWork(CancellationToken cancellationToken);
}