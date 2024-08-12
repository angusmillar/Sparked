using Abm.Sparked.Common.Validator;
using FhirNavigator;
using Abm.Sparked.eRequesting.Demo.Common.Settings;
using Abm.Sparked.eRequesting.Demo.Common.ViewModels;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Task = Hl7.Fhir.Model.Task;

namespace Abm.Sparked.eRequesting.Demo.Common.Services;

public class ServiceRequestingService(
    ILogger<ServiceRequestingService> logger,
    IOptions<WebAppSettings> webAppSettings, 
    IFhirNavigatorFactory fhirNavigatorFactory,
    IServiceRequestValidator serviceRequestValidator,
    ITaskValidator taskValidator) : IRequestingService
{
    public async Task<List<ServiceRequestVm>> GetServiceRequestVmList()
    {
        IFhirNavigator fhirNavigator = fhirNavigatorFactory.GetFhirNavigator(webAppSettings.Value.DefaultFhirRepositoryCode);
        
        var fhirQuery = new SearchParams();
        fhirQuery.Add("status", $"active");
        fhirQuery.Add("intent", $"order");
        
        SearchInfo searchInfo = await fhirNavigator.Search<ServiceRequest>(fhirQuery);
        
        List<ServiceRequestVm> serviceRequestVmList = new List<ServiceRequestVm>();

        foreach (var serviceRequest in fhirNavigator.Cache.GetList<ServiceRequest>())
        {
            ValidatorResponse validatorResponse = await serviceRequestValidator.Validate(serviceRequest,
                fhirNavigatorFactory.GetFhirNavigator(webAppSettings.Value.DefaultFhirRepositoryCode));
            if (validatorResponse.IsValid)
            {
                serviceRequestVmList.Add(ServiceRequestVm.CreateViewModel(serviceRequest));
                continue;
            }
            logger.LogWarning("Invalid ServiceRequest Id: {ResourceId}: {ErrorMessage}",serviceRequest.Id, validatorResponse.Message);
        }
        
        return serviceRequestVmList;

    }

    public async Task<string?> GetServiceRequestJson(string resourceId)
    {
        IFhirNavigator fhirNavigator = fhirNavigatorFactory.GetFhirNavigator(webAppSettings.Value.DefaultFhirRepositoryCode);

        ServiceRequest? serviceRequest = await fhirNavigator.GetResource<ServiceRequest>(resourceId: resourceId);

        if (serviceRequest is null)
        {
            return null;
        }

        var setting = FhirJsonSerializationSettings.CreateDefault();
        setting.Pretty = true;
        
        return await serviceRequest.ToJsonAsync(setting);
    }
    
    public async Task<List<TaskVm>> GetTaskVmList()
    {
        IFhirNavigator fhirNavigator = fhirNavigatorFactory.GetFhirNavigator(webAppSettings.Value.DefaultFhirRepositoryCode);
        
        var fhirQuery = new SearchParams();
        fhirQuery.Add("status", $"active");
        fhirQuery.Add("intent", $"order");
        
        SearchInfo searchInfo = await fhirNavigator.Search<Task>(fhirQuery);
        
        var taskVmList = new List<TaskVm>();

        foreach (var task in fhirNavigator.Cache.GetList<Task>())
        {
            ValidatorResponse validatorResponse = await taskValidator.Validate(task,
                fhirNavigatorFactory.GetFhirNavigator(webAppSettings.Value.DefaultFhirRepositoryCode));
            if (validatorResponse.IsValid)
            {
                taskVmList.Add(TaskVm.CreateViewModel(task));
                continue;
            }
            logger.LogWarning("Invalid Task Id: {ResourceId}: {ErrorMessage}",task.Id, validatorResponse.Message);
        }
        
        return taskVmList;

    }
}