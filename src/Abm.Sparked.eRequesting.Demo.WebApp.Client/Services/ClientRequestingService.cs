using System.Net.Http.Json;
using Abm.Sparked.eRequesting.Demo.Common.Services;
using Abm.Sparked.eRequesting.Demo.Common.ViewModels;

namespace Abm.Sparked.eRequesting.Demo.WebApp.Client.Services;

public class ClientRequestingService(HttpClient http) : IRequestingService
{
    public async Task<List<ServiceRequestVm>> GetServiceRequestVmList()
    {
        var serviceRequestVmList = await http.GetFromJsonAsync<List<ServiceRequestVm>>("serviceRequestVms");
        return serviceRequestVmList ?? Array.Empty<ServiceRequestVm>().ToList();
    }
    
    public async Task<string?> GetServiceRequestJson(string resourceId)
    {
        return await http.GetStringAsync($"serviceRequestJson/{resourceId}");
    }

    public async Task<List<TaskVm>> GetTaskVmList()
    {
        var serviceRequestVmList = await http.GetFromJsonAsync<List<TaskVm>>("taskVms");
        return serviceRequestVmList ?? Array.Empty<TaskVm>().ToList();
    }
}