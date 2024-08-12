using Abm.Sparked.Common.Support;
using FhirNavigator;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;

namespace Abm.Sparked.Common.Validator;

public class TaskValidator(IServiceRequestValidator serviceRequestValidator) : ValidatorBase, ITaskValidator
{
    
    private IFhirNavigator? _fhirNavigator;
    
    public async Task<ValidatorResponse> Validate(Hl7.Fhir.Model.Task task, IFhirNavigator? fhirNavigator = null)
    {
        _fhirNavigator = fhirNavigator;
        
        var validatorResponseList = new List<ValidatorResponse>();

        validatorResponseList.Add(ValidateGroupIdentifier(task.GroupIdentifier));
        validatorResponseList.Add(ValidateReferencePopulated(task.Focus, "Task.focus"));
        
        validatorResponseList.Add(await ValidateFocusReference(task));
        
        return ConsolidatedValidationResponse(validatorResponseList);
        
    }
    
    private ValidatorResponse ValidateGroupIdentifier(Identifier? identifier )
    {
        if (identifier is null)
        {
            return GetInvalidResponse(message: "Task.groupIdentifier SHALL NOT be empty");    
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