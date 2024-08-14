using FhirNavigator;
using Abm.Sparked.Common.Constants;
using Abm.Sparked.Common.Support;
using Abm.Sparked.Common.Validator;
using Abm.Sparked.eRequesting.Demo.Common.HostedServiceSupport;
using Abm.Sparked.eRequesting.Demo.Common.Settings;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Task = System.Threading.Tasks.Task;

namespace Abm.Sparked.eRequesting.Demo.Common.Managers;

public class FhirTaskManager(ILogger<FhirTaskManager> logger, 
    IOptions<WebAppSettings> webAppSettings, 
    IFhirNavigatorFactory fhirNavigatorFactory,
    IServiceRequestValidator serviceRequestValidator) : ITimedHostedService
{
    private IFhirNavigator? _fhirNavigator;
    public async Task DoWork(CancellationToken cancellationToken)
    {
        
        _fhirNavigator = fhirNavigatorFactory.GetFhirNavigator(webAppSettings.Value.DefaultFhirRepositoryCode);

        string? fillerHpioValue = await GetFillerHpioIdentifierValue();
        if (fillerHpioValue is null)
        {
            logger.LogError("Unable to to process Task resources as no Filler HPI-O value found");
        }
        
        var fhirQuery = new SearchParams();
        fhirQuery.Add("status", $"requested");
        fhirQuery.Add("intent", $"order");
        fhirQuery.Add("owner:Organization.identifier", $"{IdentifiersConstants.HpioTypeSystem}|{fillerHpioValue}");
        
        SearchInfo searchInfo = await _fhirNavigator.Search<Hl7.Fhir.Model.Task>(fhirQuery);
        logger.LogInformation("Processing {TaskCount} Tasks for Filler HPI-O: {FillerHpioValue}", searchInfo.ResourceTotal, fillerHpioValue);
        foreach (Hl7.Fhir.Model.Task task in _fhirNavigator.Cache.GetList<Hl7.Fhir.Model.Task>())
        {
            await ProcessFhirTask(task);
        }
    }

    private async Task<string?> GetFillerHpioIdentifierValue()
    {
        Organization? organization = await GetFillerOrganizationResource();
        if (organization is null)
        {
            return null;
        }

        Identifier? hpioIdentifier = organization.Identifier.FirstOrDefault(x =>
            x.System.Equals(IdentifiersConstants.HpioSystem, StringComparison.OrdinalIgnoreCase));

        if (hpioIdentifier is null)
        {
            logger.LogError("Unable to locate a HPI-O identifier in the Organization.identifier list for the the Filler Organization Resource with Id: {ResourceId}", webAppSettings.Value.FillerOrganizationResourceId);
            return null;
        }

        return hpioIdentifier.Value.RemoveWhitespace();

    }

    private async Task<Organization?> GetFillerOrganizationResource()
    {
        ArgumentNullException.ThrowIfNull(_fhirNavigator);
        
        Organization? fillerOrganization = await _fhirNavigator.GetResource<Organization>(resourceId: webAppSettings.Value
            .FillerOrganizationResourceId);

        if (fillerOrganization is null)
        {
            logger.LogError("Unable to locate the Filler Organization Resource with Id: {ResourceId}", webAppSettings.Value.FillerOrganizationResourceId);
        }

        return fillerOrganization;
    }

    private async Task ProcessFhirTask(Hl7.Fhir.Model.Task task)
    {
        ArgumentNullException.ThrowIfNull(_fhirNavigator);
       
        ServiceRequest? serviceRequest = await _fhirNavigator.GetResource<ServiceRequest>(task.Focus, "Task.focus", task);
       
        if (serviceRequest is null)
        {
            await UpdateTaskStatus(task, Hl7.Fhir.Model.Task.TaskStatus.Rejected, "Task.focus must be a resource reference to a ServiceRequest resource type");
            return;
        }

        IFhirNavigator validationFhirNavigator =
            fhirNavigatorFactory.GetFhirNavigator(webAppSettings.Value.DefaultFhirRepositoryCode);
        
        ValidatorResponse serviceRequestValidatorResponse = await serviceRequestValidator.Validate(serviceRequest, fhirNavigator: validationFhirNavigator);
        if (!serviceRequestValidatorResponse.IsValid)
        {
            await UpdateTaskStatus(task, Hl7.Fhir.Model.Task.TaskStatus.Rejected, $"Task.focus ServiceRequest resource failed validation. {serviceRequestValidatorResponse.Message}");
            return;
        }
        
        await UpdateTaskStatus(task, Hl7.Fhir.Model.Task.TaskStatus.Accepted, $"Task has been Accepted");
        
    }
    
    private async Task UpdateTaskStatus(Hl7.Fhir.Model.Task task, Hl7.Fhir.Model.Task.TaskStatus status, string? statusReason = null)
    {
        ArgumentNullException.ThrowIfNull(_fhirNavigator);
        
        task.Status = status;
        if (statusReason is not null)
        {
            task.StatusReason = new CodeableConcept() { Text = statusReason};    
        }
        task = await _fhirNavigator.UpdateResource(task);
        logger.LogInformation("Task/{TaskResourceId}, Status: {Status}, StatusReason: {StatusReason}", task.Status.ToString(), task.Id, statusReason);
    }
}