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
    
}