namespace Abm.Sparked.eRequesting.Demo.Common.HostedServiceSupport;

public interface IAppStartupService
{
    public Task DoWork(CancellationToken cancellationToken);
}