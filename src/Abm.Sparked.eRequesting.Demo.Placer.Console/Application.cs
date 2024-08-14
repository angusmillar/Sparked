using Abm.Sparked.Common.Constants;
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
    IServiceRequestValidator serviceRequestValidator,
    ITaskValidator taskValidator)
{
    private IFhirNavigator? _fhirNavigator;

    public async Task Run()
    {
        _fhirNavigator = fhirNavigatorFactory.GetFhirNavigator(appConfig.Value.DefaultFhirRepositoryCode);
        
        List<Resource> resourceList1 = GetPathologyServiceRequestList();
        if (await ResourceListValidationPassed(resourceList1))
        {
            await CreateOrUpdateResourceList(resourceList1);
        }
        
        
        List<Resource> resourceList2 = GetTaskBasedPathologyRequestResourceList();
        if (await ResourceListValidationPassed(resourceList1))
        {
            await CreateOrUpdateResourceList(resourceList1);
        }
        
        logger.LogInformation("Successfully Created or Updated the Sparked example ServiceRequest resources");
    }

    private List<Resource> GetPathologyServiceRequestList()
    {
        string placerScopeingHpio = "8003629900040425";
        string barcodeValue = "SONIC-0002";
        string resourceIdPrefix = "sonic-0002";

        ResourceReference patientReference = GetResourceReference(
            ResourceType.ServiceRequest,
            resourceId: "moylan-brock",
            "MOYLAN, Brock");

        ResourceReference requesterReference = GetResourceReference(
            ResourceType.PractitionerRole,
            resourceId: "generalpractitioner-lumb-mary",
            display: "Dr LOWE, Abe");

        var testList = new List<SelectedTestInput>()
        {
            new SelectedTestInput(displayPrefix: "FBC",
                CodeableConcept: CodeableConceptSupport.GetRequestedSnomedTest(
                    term: "26604007",
                    preferredDisplay: "Full blood count")),
            new SelectedTestInput(displayPrefix: "IM",
                CodeableConcept: CodeableConceptSupport.GetRequestedSnomedTest(
                    term: "269831005",
                    preferredDisplay: "Infectious mononucleosis test")),
            new SelectedTestInput(displayPrefix: "Rhubarb",
                CodeableConcept: CodeableConceptSupport.GetRequestedFreeTextTest("Serum Rhubarb"))
        };

        List<PathologyServiceRequestInput> pathologyServiceRequestInputList = GetPathologyServiceRequestInputList(
            resourceIdPrefix: resourceIdPrefix,
            requisition: IdentifierSupport.GetRequisition(scopeingHpio: placerScopeingHpio,
                value: barcodeValue),
            requestedDateTimeOffset: new DateTimeOffset(2024, 08, 20, 10, 30, 00, TimeSpan.FromHours(10)),
            patientReference: patientReference,
            requesterReference: requesterReference,
            selectedTestInputList: testList);

        var serviceRequestList = new List<Resource>();
        foreach (var pathologyServiceRequestInput in pathologyServiceRequestInputList)
        {
            serviceRequestList.Add(
                PathologyServiceRequestFactory.GetServiceRequest(input: pathologyServiceRequestInput));
        }

        return serviceRequestList;
    }

    private List<Resource> GetTaskBasedPathologyRequestResourceList()
    {
        ArgumentNullException.ThrowIfNull(_fhirNavigator);

        string placerScopeingHpio = "8003629900040425";
        string barcodeValue = "SONIC-0002";
        string resourceIdPrefix = "sonic-0002";
        var requestedDateTime = new DateTimeOffset(2024, 08, 20, 10, 30, 00, TimeSpan.FromHours(10));

        ResourceReference patientReference = GetResourceReference(
            ResourceType.ServiceRequest,
            resourceId: "moylan-brock",
            "MOYLAN, Brock");

        ResourceReference requesterReference = GetResourceReference(
            ResourceType.PractitionerRole,
            resourceId: "generalpractitioner-lumb-mary",
            display: "Dr LOWE, Abe");

        ResourceReference fillerOrganizationReference = GetResourceReference(
            ResourceType.PractitionerRole,
            resourceId: "pullabooka-pathology",
            display: "Pullabooka Pathology");

        var testList = new List<SelectedTestInput>()
        {
            new SelectedTestInput(displayPrefix: "FBC",
                CodeableConcept: CodeableConceptSupport.GetRequestedSnomedTest(
                    term: "26604007",
                    preferredDisplay: "Full blood count")),
            new SelectedTestInput(displayPrefix: "IM",
                CodeableConcept: CodeableConceptSupport.GetRequestedSnomedTest(
                    term: "269831005",
                    preferredDisplay: "Infectious mononucleosis test")),
            new SelectedTestInput(displayPrefix: "Rhubarb",
                CodeableConcept: CodeableConceptSupport.GetRequestedFreeTextTest("Serum Rhubarb")),
            new SelectedTestInput(displayPrefix: "MetaPanel", CodeableConcept: new CodeableConcept(
                system: "https://terminology.sonichealthcare.com.au/CodeSystem/sonic-pathology-local-order-codes",
                code: "A000210",
                text: "MetaPanel - Faecal metagenomics panel")),
        };


        List<PathologyServiceRequestInput> serviceRequestInputList = GetPathologyServiceRequestInputList(
            resourceIdPrefix: resourceIdPrefix,
            requisition: IdentifierSupport.GetRequisition(scopeingHpio: placerScopeingHpio,
                value: barcodeValue),
            requestedDateTimeOffset: requestedDateTime,
            patientReference: patientReference,
            requesterReference: requesterReference,
            selectedTestInputList: testList);

        List<PathologyTaskInput> pathologyTaskInputList = GetPathologyTaskInputList(
            resourceIdPrefix: resourceIdPrefix,
            owner: fillerOrganizationReference,
            pathologyServiceRequestInputList: serviceRequestInputList);


        var resourceList = new List<Resource>();
        foreach (var pathologyTaskInput in pathologyTaskInputList)
        {
            resourceList.Add(
                PathologyTaskFactory.GetTask(input: pathologyTaskInput));
        }

        foreach (var serviceRequestInput in serviceRequestInputList)
        {
            resourceList.Add(
                PathologyServiceRequestFactory.GetServiceRequest(input: serviceRequestInput));
        }

        return resourceList;
    }

    private List<PathologyTaskInput> GetPathologyTaskInputList(
        string resourceIdPrefix, 
        ResourceReference owner, 
        List<PathologyServiceRequestInput> pathologyServiceRequestInputList)
    {
        var taskList = new List<PathologyTaskInput>();
        foreach (var input in pathologyServiceRequestInputList)
        {
            taskList.Add(new PathologyTaskInput(
                ResourceId: $"{resourceIdPrefix}-task",
                GroupIdentifier: input.Requisition,
                RequestStatus: Hl7.Fhir.Model.Task.TaskStatus.Requested,
                Intent: Hl7.Fhir.Model.Task.TaskIntent.Order,
                Code: new CodeableConcept(system: CodeSystemsConstants.TaskCodeSystem, code: "fulfill",
                    text: "Fulfill the focal request"),
                AuthoredOn: input.requestedDateTime,
                Focus: GetResourceReference(ResourceType.ServiceRequest, resourceId: input.ResourceId, display: null),
                For: input.PatientReference,
                Owner: owner,
                Requester: input.RequesterPractitionerRoleReference
                ));
        }

        return taskList;
    }

    private async Task CreateOrUpdateResourceList(List<Resource> resourceList)
    {
        ArgumentNullException.ThrowIfNull(_fhirNavigator);
        
        foreach (var resource in resourceList)
        {
            await _fhirNavigator.UpdateResource(resource);
            logger.LogInformation("Updated: {ResourceType}/{ResourceId}", resource.TypeName, resource.Id);
        }
    }
    
    private async Task<bool> ResourceListValidationPassed(List<Resource> resourceList)
    {
        IFhirNavigator validationFhirNavigator =
            fhirNavigatorFactory.GetFhirNavigator(appConfig.Value.DefaultFhirRepositoryCode);
        
        foreach (var resource in resourceList)
        {
            ValidatorResponse? validatorResponse = resource switch
            {
                ServiceRequest serviceRequest => await serviceRequestValidator.Validate(serviceRequest,
                    fhirNavigator: validationFhirNavigator),
                Hl7.Fhir.Model.Task task => await taskValidator.Validate(task, 
                    fhirNavigator: validationFhirNavigator),
                _ => null
            };

            ArgumentNullException.ThrowIfNull(validatorResponse);
            
            if (validatorResponse.IsValid)
            {
                logger.LogCritical(
                    "{ResourceName} Resource Id: {ResourceId} has the following validation errors: {Errors}",
                    resource.TypeName, resource.Id, validatorResponse.Message);
                return false;
            }
            
            logger.LogInformation("Validated: {ResourceName}/{ResourceId} ", resource.TypeName,
                resource.Id);    
            
        }

        return true;
    }

    private static List<PathologyServiceRequestInput> GetPathologyServiceRequestInputList(
        string resourceIdPrefix,
        Identifier requisition,
        DateTimeOffset requestedDateTimeOffset,
        ResourceReference patientReference,
        ResourceReference requesterReference,
        List<SelectedTestInput> selectedTestInputList)
    {
        var pathologyServiceRequestInputList = new List<PathologyServiceRequestInput>();
        foreach (var selectedTestInput in selectedTestInputList)
        {
            pathologyServiceRequestInputList.Add(new PathologyServiceRequestInput(
                ResourceId: $"{resourceIdPrefix}-{selectedTestInput.displayPrefix}",
                Requisition: requisition,
                RequestedTest: selectedTestInput.CodeableConcept,
                requestedDateTime: requestedDateTimeOffset,
                PatientReference: patientReference,
                RequesterPractitionerRoleReference: requesterReference
            ));
        }

        return pathologyServiceRequestInputList;
    }

    private static ResourceReference GetResourceReference(ResourceType resourceType, string resourceId,
        string? display = null)
    {
        return new ResourceReference($"{resourceType.GetLiteral()}/{resourceId}", display: display);
    }
}