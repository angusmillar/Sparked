using FhirNavigator;
using Abm.Sparked.eRequesting.Demo.Common.Settings;
using Abm.Sparked.eRequesting.Demo.Common.ViewModels;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Options;

namespace Abm.Sparked.eRequesting.Demo.Common.Services;

public class ServiceRequestingService(IOptions<WebAppSettings> webAppSettings, IFhirNavigatorFactory fhirNavigatorFactory) : IRequestingService
{
    public async Task<List<ServiceRequestVm>> GetServiceRequestVmList()
    {
        IFhirNavigator fhirNavigator = fhirNavigatorFactory.GetFhirNavigator(webAppSettings.Value.DefaultFhirRepositoryCode);
        
        var fhirQuery = new SearchParams();

        //groupSearchParams.Add("status", $"Requested");
        fhirQuery.Add("status", $"active");
        fhirQuery.Add("intent", $"order");
        
        SearchInfo searchInfo = await fhirNavigator.Search<ServiceRequest>(fhirQuery);
        
        List<ServiceRequestVm> serviceRequestVmList = new List<ServiceRequestVm>();
        fhirNavigator.Cache.GetList<ServiceRequest>().ForEach(x => serviceRequestVmList.Add(ServiceRequestVm.CreateViewModel(x)));
        
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
}