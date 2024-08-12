using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.Validator;

public abstract class ValidatorBase
{
    protected static ValidatorResponse ConsolidatedValidationResponse(List<ValidatorResponse> validatorResponseList)
    {
        if (validatorResponseList.Any(x => !x.IsValid))
        {
            return GetInvalidResponse(string.Join(", ",
                validatorResponseList.Where(a => !a.IsValid).Select(s => s.Message)));
            
        }

        return GetSuccessfulResponse();
    }
    protected  static ValidatorResponse GetInvalidResponse(string message)
    {
        return new ValidatorResponse(IsValid: false, Message: message);
    }
    protected  static ValidatorResponse GetSuccessfulResponse()
    {
        return new ValidatorResponse(IsValid: true);
    }
    
    protected ValidatorResponse ValidateReferencePopulated(ResourceReference? serviceRequestSubject, string propertyLocation)
    {
        if (serviceRequestSubject is null)
        {
            return GetInvalidResponse(message: $"{propertyLocation} SHALL NOT be empty");    
        }
        
        if (serviceRequestSubject.Reference is null)
        {
            return GetInvalidResponse(message: $"{propertyLocation} SHALL NOT be empty");    
        }
        
        return GetSuccessfulResponse();
    }
    
}