namespace Abm.Sparked.Common.HostedServiceSupport;

public interface IAppStartupService
{
    public Task DoWork(CancellationToken cancellationToken);
}