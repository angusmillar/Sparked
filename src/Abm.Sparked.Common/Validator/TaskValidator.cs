using Abm.Sparked.Common.Constants;
using FhirNavigator;
using Hl7.Fhir.Model;
using Task = Hl7.Fhir.Model.Task;

namespace Abm.Sparked.Common.Validator;

public class TaskValidator(
    IServiceRequestValidator serviceRequestValidator,
    IPractitionerRoleValidator practitionerRoleValidator, 
    IOrganizationRoleValidator organizationRoleValidator) : ValidatorBase, ITaskValidator
{
    
    private IFhirNavigator? _fhirNavigator;
    
    public async Task<ValidatorResponse> Validate(Hl7.Fhir.Model.Task task, IFhirNavigator? fhirNavigator = null)
    {
        _fhirNavigator = fhirNavigator;
        
        var validatorResponseList = new List<ValidatorResponse>();

        validatorResponseList.Add(ValidateGroupIdentifier(task.GroupIdentifier));
        validatorResponseList.Add(ValidateReferencePopulated(task.Focus, "Task.focus"));
        validatorResponseList.Add(await ValidateFocusReference(task));
        validatorResponseList.Add(ValidateStatus(task.Status));
        validatorResponseList.Add(ValidateIntent(task.Intent));
        validatorResponseList.Add(ValidateCode(task.Code));
        validatorResponseList.Add(ValidateAuthoredOn(task.AuthoredOnElement));
        validatorResponseList.Add(ValidateReferencePopulated(task.Requester, "Task.requester"));
        validatorResponseList.Add(await ValidateRequesterReference(task));
        validatorResponseList.Add(ValidateReferencePopulated(task.Owner, "Task.owner"));
        validatorResponseList.Add(await ValidateOwnerReference(task));

        return ConsolidatedValidationResponse(validatorResponseList);
        
    }

    private async Task<ValidatorResponse> ValidateOwnerReference(Task task)
    {
        if (task.Owner is null)
        {
            return GetInvalidResponse(message: "Task.owner SHALL NOT be empty");
        }
        
        if (string.IsNullOrWhiteSpace(task.Requester.Reference))
        {
            return GetInvalidResponse(message: "Task.owner SHALL NOT be empty");
        }
        
        if (_fhirNavigator is null)
        {
            return GetSuccessfulResponse();
        }
        
        Organization? organization = await _fhirNavigator.GetResource<Organization>(task.Requester, "Task.owner", task);
        if (organization is null)
        {
            return GetInvalidResponse(message: "Task.owner unable to resolve the organization resource from the default FHIR Repository");
        }
        
        ValidatorResponse organizationValidatorResponse = await organizationRoleValidator.Validate(organization, _fhirNavigator);
        if (!organizationValidatorResponse.IsValid)
        {
            return organizationValidatorResponse;
        }

        return GetSuccessfulResponse();
    }

    private async Task<ValidatorResponse> ValidateRequesterReference(Task task)
    {
        if (task.Requester is null)
        {
            return GetInvalidResponse(message: "Task.requester SHALL NOT be empty");
        }
        
        if (string.IsNullOrWhiteSpace(task.Requester.Reference))
        {
            return GetInvalidResponse(message: "Task.requester SHALL NOT be empty");
        }
        
        if (_fhirNavigator is null)
        {
            return GetSuccessfulResponse();
        }
        
        PractitionerRole? practitionerRole = await _fhirNavigator.GetResource<PractitionerRole>(task.Requester, "Task.requester", task);
        if (practitionerRole is null)
        {
            return GetInvalidResponse(message: "Task.requester unable to resolve the PractitionerRole resource from the default FHIR Repository");
        }
        
        ValidatorResponse practitionerRoleValidatorResponse = await practitionerRoleValidator.Validate(practitionerRole, _fhirNavigator);
        if (!practitionerRoleValidatorResponse.IsValid)
        {
            return practitionerRoleValidatorResponse;
        }

        return GetSuccessfulResponse();
    }

    private ValidatorResponse ValidateAuthoredOn(FhirDateTime authoredOn)
    {
        if (string.IsNullOrWhiteSpace(authoredOn.Value))
        {
            return GetInvalidResponse(message: "Task.authoredOn SHALL NOT be empty");    
        }

        try
        {
            authoredOn.ToDateTime();
        }
        catch (FormatException)
        {
            return GetInvalidResponse(message: $"Task.authoredOn was found to be an invalid format: {authoredOn.Value}");    
        }
        
        return GetSuccessfulResponse();
    }
    
    private ValidatorResponse ValidateCode(CodeableConcept? taskCode)
    {
        if (taskCode is null)
        {
            return GetInvalidResponse(message: "Task.code SHALL NOT be empty");
        }

        string code = "fullfill";
        if (taskCode.Coding.Any(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase) &&
                                     x.System.Equals(CodeSystemsConstants.TaskCodeSystem, StringComparison.OrdinalIgnoreCase)))
        {
            return GetInvalidResponse(message: $"Task.code SHALL be set to {code} with a system of {CodeSystemsConstants.TaskCodeSystem}");
        }
        
        return GetSuccessfulResponse();
    }

    private ValidatorResponse ValidateIntent(Task.TaskIntent? taskIntent)
    {
        if (taskIntent is null)
        {
            return GetInvalidResponse(message: "Task.intent SHALL NOT be empty");
        }
        
        if (taskIntent.Equals(Task.TaskIntent.Order))
        {
            return GetInvalidResponse(message: "Task.intent SHALL be set to order");
        }
        
        return GetSuccessfulResponse();
    }

    private ValidatorResponse ValidateStatus(Task.TaskStatus? taskStatus)
    {
        if (taskStatus is null)
        {
            return GetInvalidResponse(message: "Task.status SHALL NOT be empty");
        }
        
        return GetSuccessfulResponse();
    }

    private ValidatorResponse ValidateGroupIdentifier(Identifier? identifier )
    {
        if (identifier is null)
        {
            return GetInvalidResponse(message: "Task.groupIdentifier SHALL NOT be empty");    
        }

        if (string.IsNullOrWhiteSpace(identifier.Value))
        {
            return GetInvalidResponse(message: "Task.groupIdentifier.value SHALL NOT be empty");
        }
        
        if (string.IsNullOrWhiteSpace(identifier.System))
        {
            return GetInvalidResponse(message: "Task.groupIdentifier.system SHALL NOT be empty");
        }
        
        return GetSuccessfulResponse();

    }
    private async Task<ValidatorResponse> ValidateFocusReference(Hl7.Fhir.Model.Task task)
    {
        if (task.Focus is null)
        {
            return GetInvalidResponse(message: "Task.focus SHALL NOT be empty");
        }
        
        if (string.IsNullOrWhiteSpace(task.Focus.Reference))
        {
            return GetInvalidResponse(message: "Task.focus SHALL NOT be empty");
        }
        
        if (_fhirNavigator is null)
        {
            return GetSuccessfulResponse();
        }
        
        ServiceRequest? serviceRequest = await _fhirNavigator.GetResource<ServiceRequest>(task.Focus, "Task.focus", task);
        if (serviceRequest is null)
        {
            return GetInvalidResponse(message: "Task.focus unable to resolve the ServiceRequest resource from the default FHIR Repository");
        }
        
        ValidatorResponse serviceRequestValidatorResponse = await serviceRequestValidator.Validate(serviceRequest, _fhirNavigator);
        if (!serviceRequestValidatorResponse.IsValid)
        {
            return serviceRequestValidatorResponse;
        }

        return GetSuccessfulResponse();
    }
    
    
    
}