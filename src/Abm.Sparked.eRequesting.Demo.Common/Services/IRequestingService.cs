using Abm.Sparked.eRequesting.Demo.Common.ViewModels;

namespace Abm.Sparked.eRequesting.Demo.Common.Services;

public interface IRequestingService
{
    Task<List<ServiceRequestVm>> GetServiceRequestVmList();

    Task<string?> GetServiceRequestJson(string resourceId);
    Task<List<TaskVm>> GetTaskVmList();

}