using System.Collections;
using FhirNavigator;
using Abm.Sparked.Common.eRequesting;
using Abm.Sparked.Common.Support;
using Abm.Sparked.Common.Validator;
using ConsoleApp;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Task = System.Threading.Tasks.Task;

namespace Abm.Sparked.eRequesting.Demo.Placer.Console;

public class Application(
    ILogger<Application> logger,
    IOptions<ApplicationConfiguration> appConfig,
    IFhirNavigatorFactory fhirNavigatorFactory,
    IServiceRequestValidator serviceRequestValidator)
{
    private IFhirNavigator? _fhirNavigator;

    public async Task Run()
    {
        _fhirNavigator = fhirNavigatorFactory.GetFhirNavigator(appConfig.Value.DefaultFhirRepositoryCode);

        await CreateOrUpdateServiceRequestResourceList();
        
        await CreateOrUpdateTaskResourceList();

        logger.LogInformation("Successfully Created or Updated the Sparked example ServiceRequest resources");
    }

    private async Task CreateOrUpdateTaskResourceList()
    {
        ArgumentNullException.ThrowIfNull(_fhirNavigator);
        List<Hl7.Fhir.Model.Task> TaskList = GetTaskList();
        // if (!await ValidateServiceRequestList(serviceRequestList))
        // {
        //     logger.LogError("No ServiceRequest resources were updated due to a failed validation");
        //     return;
        // }
        //
        // foreach (var serviceRequest in serviceRequestList)
        // {
        //     await _fhirNavigator.UpdateResource(serviceRequest);
        //     logger.LogInformation("Updated: {ResourceType}/{ResourceId}", serviceRequest.TypeName, serviceRequest.Id);
        // }
    }
    
    private async Task CreateOrUpdateServiceRequestResourceList()
    {
        ArgumentNullException.ThrowIfNull(_fhirNavigator);

        List<ServiceRequest> serviceRequestList = GetServiceRequestList();
        if (!await ValidateServiceRequestList(serviceRequestList))
        {
            logger.LogError("No ServiceRequest resources were updated due to a failed validation");
            return;
        }

        foreach (var serviceRequest in serviceRequestList)
        {
            await _fhirNavigator.UpdateResource(serviceRequest);
            logger.LogInformation("Updated: {ResourceType}/{ResourceId}", serviceRequest.TypeName, serviceRequest.Id);
        }
    }

    private async Task<bool> ValidateServiceRequestList(List<ServiceRequest> serviceRequestList)
    {
        bool isValid = true;
        IFhirNavigator validationFhirNavigator =
            fhirNavigatorFactory.GetFhirNavigator(appConfig.Value.DefaultFhirRepositoryCode);
        foreach (var serviceRequest in serviceRequestList)
        {
            ValidatorResponse validatorResponse =
                await serviceRequestValidator.Validate(serviceRequest, fhirNavigator: validationFhirNavigator);
            if (!validatorResponse.IsValid)
            {
                logger.LogCritical(
                    "ServiceRequest Resource Id: {ResourceId} has the following validation errors: {Errors}",
                    serviceRequest.Id, validatorResponse.Message);
                isValid = false;
            }
            logger.LogInformation("Validated: {ResourceName}/{ResourceId} ",serviceRequest.TypeName, serviceRequest.Id);
        }

        return isValid;
    }

    private List<ServiceRequest> GetServiceRequestList()
    {
        var serviceRequestList = new List<ServiceRequest>();
        foreach (var pathologyServiceRequestInput in PathologyServiceRequestInputList())
        {
            serviceRequestList.Add(
                PathologyServiceRequestFactory.GetServiceRequest(input: pathologyServiceRequestInput));
        }

        return serviceRequestList;
    }

    private List<Hl7.Fhir.Model.Task> GetTaskList()
    {
        var taskList = new List<Hl7.Fhir.Model.Task>();
        foreach (var pathologyServiceRequestInput in PathologyTaskInputList())
        {
            // taskList.Add(
            //     PathologyServiceRequestFactory.GetServiceRequest(input: pathologyServiceRequestInput));
        }

        return taskList;
    }
    
    private static List<PathologyServiceRequestInput> PathologyServiceRequestInputList()
    {
        string placerScopeingHpio = "8003629900040425";
        return new List<PathologyServiceRequestInput>()
        {
            //Example One
            new PathologyServiceRequestInput(
                ResourceId: "angus-serviceRequest-1",
                Requisition: IdentifierSupport.GetRequisition(scopeingHpio: placerScopeingHpio,
                    value: "AAA0000-0000001"),
                RequestedTest: CodeableConceptSupport.GetRequestedSnomedTest(term: "26604007",
                    preferredDisplay: "Full blood count"),
                requestedDateTime: new DateTimeOffset(2024, 08, 20, 10, 30, 00, TimeSpan.FromHours(10)),
                PatientReference: GetResourceReference(ResourceType.Patient, resourceId: "moylan-brock",
                    "MOYLAN, Brock"),
                RequesterPractitionerRoleReference: GetResourceReference(ResourceType.PractitionerRole,
                    resourceId: "generalpractitioner-lowe-abe", display: "Dr LOWE, Abe")
            ),
            //Example Two
            new PathologyServiceRequestInput(
                ResourceId: "angus-serviceRequest-2",
                Requisition: IdentifierSupport.GetRequisition(scopeingHpio: "8003624900039295",
                    value: "BBB0000-0000002"),
                RequestedTest: CodeableConceptSupport.GetRequestedSnomedTest(term: "26604007",
                    preferredDisplay: "Full blood count"),
                requestedDateTime: new DateTimeOffset(2024, 08, 20, 10, 35, 00, TimeSpan.FromHours(10)),
                PatientReference: GetResourceReference(ResourceType.Patient, resourceId: "ralph-rudolf",
                    "RALPH, Rudolf"),
                RequesterPractitionerRoleReference: GetResourceReference(ResourceType.PractitionerRole,
                    resourceId: "generalpractitioner-faint-darryl", display: "Dr FAINT, Darryl")
            ),
            //Example Two
            new PathologyServiceRequestInput(
                ResourceId: "angus-serviceRequest-3",
                Requisition: IdentifierSupport.GetRequisition(scopeingHpio: placerScopeingHpio,
                    value: "CCC0000-0000003"),
                RequestedTest: CodeableConceptSupport.GetRequestedSnomedTest(term: "26604007",
                    preferredDisplay: "Full blood count"),
                requestedDateTime: new DateTimeOffset(2024, 08, 20, 10, 35, 00, TimeSpan.FromHours(10)),
                PatientReference: GetResourceReference(ResourceType.Patient, resourceId: "italia-sofia",
                    "ITALAIA, Sofia"),
                RequesterPractitionerRoleReference: GetResourceReference(ResourceType.PractitionerRole,
                    resourceId: "generalpractitioner-lumb-mary", display: "Dr LOWE, Abe")
            )
        };
    }
    
    private static List<PathologyTaskInput> PathologyTaskInputList()
    {
        string placerScopeingHpio = "8003629900040425"; 
        return new List<PathologyTaskInput>()
        {
            //Example One
            new PathologyTaskInput(
                ResourceId: "angus-task-1",
                GroupIdentifier: IdentifierSupport.GetRequisition(scopeingHpio: placerScopeingHpio,
                    value: "AAA0000-0000001"),
                RequestStatus: Hl7.Fhir.Model.Task.TaskStatus.Requested,
                Intent: Hl7.Fhir.Model.Task.TaskIntent.Order,
                Code: new CodeableConcept(),
                AuthoredOn: new DateTimeOffset(2024, 08, 20, 10, 30, 00, TimeSpan.FromHours(10)), 
                Focus: GetResourceReference(ResourceType.ServiceRequest, resourceId: "moylan-brock",
                    "MOYLAN, Brock"),
                Owner: GetResourceReference(ResourceType.Organization, resourceId: "moylan-brock", 
                    "MOYLAN, Brock"),
                Requester: GetResourceReference(ResourceType.PractitionerRole, resourceId: "moylan-brock",
                    "MOYLAN, Brock")
            ),
            //Example Two
            new PathologyTaskInput(
                ResourceId: "angus-task-1",
                GroupIdentifier: IdentifierSupport.GetRequisition(scopeingHpio: placerScopeingHpio,
                    value: "AAA0000-0000001"),
                RequestStatus: Hl7.Fhir.Model.Task.TaskStatus.Requested,
                Intent: Hl7.Fhir.Model.Task.TaskIntent.Order,
                Code: new CodeableConcept(),
                AuthoredOn: new DateTimeOffset(2024, 08, 20, 10, 30, 00, TimeSpan.FromHours(10)), 
                Focus: GetResourceReference(ResourceType.ServiceRequest, resourceId: "moylan-brock",
                    "MOYLAN, Brock"),
                Owner: GetResourceReference(ResourceType.Organization, resourceId: "moylan-brock",
                    "MOYLAN, Brock"),
                Requester: GetResourceReference(ResourceType.PractitionerRole, resourceId: "moylan-brock",
                    "MOYLAN, Brock")
            ),
            //Example Two
            new PathologyTaskInput(
                ResourceId: "angus-task-1",
                GroupIdentifier: IdentifierSupport.GetRequisition(scopeingHpio: placerScopeingHpio,
                    value: "AAA0000-0000001"),
                RequestStatus: Hl7.Fhir.Model.Task.TaskStatus.Requested,
                Intent: Hl7.Fhir.Model.Task.TaskIntent.Order,
                Code: new CodeableConcept(),
                AuthoredOn: new DateTimeOffset(2024, 08, 20, 10, 30, 00, TimeSpan.FromHours(10)), 
                Focus: GetResourceReference(ResourceType.ServiceRequest, resourceId: "moylan-brock",
                    "MOYLAN, Brock"),
                Owner: GetResourceReference(ResourceType.Organization, resourceId: "moylan-brock",
                    "MOYLAN, Brock"),
                Requester: GetResourceReference(ResourceType.PractitionerRole, resourceId: "moylan-brock",
                    "MOYLAN, Brock")
            ),
        };
    }

    private static ResourceReference GetResourceReference(ResourceType resourceType, string resourceId, string? display)
    {
        return new ResourceReference($"{resourceType.GetLiteral()}/{resourceId}", display: display);
    }
}