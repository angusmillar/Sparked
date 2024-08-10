using Abm.Sparked.Common.Support;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;

namespace Abm.Sparked.Common.Validator;

public class PatientValidator : ValidatorBase, IPatientValidator
{
    public ValidatorResponse Validate(Patient patient)
    {
        var validatorResponseList = new List<ValidatorResponse>();

        validatorResponseList.Add(ValidateHumanNameList(patient.Name));
       
        
        return ConsolidatedValidationResponse(validatorResponseList);
        
    }
    
    private ValidatorResponse ValidateHumanNameList(List<HumanName>? humanNameList )
    {
        if (humanNameList is null)
        {
            return GetInvalidResponse(message: "Patient.name SHALL NOT be empty");    
        }
        
        if (!humanNameList.Any())
        {
            return GetInvalidResponse(message: "Patient.name SHALL NOT be empty");    
        }
        
        var validatorResponseList = new List<ValidatorResponse>();
        foreach (var humanName in humanNameList)
        {
            validatorResponseList.Add(ValidateHumanName(humanName));
        }
        
        return ConsolidatedValidationResponse(validatorResponseList);

    }

    private static ValidatorResponse ValidateHumanName(HumanName humanName)
    {
        if (string.IsNullOrWhiteSpace(humanName.Text))
        {
            return GetSuccessfulResponse();
        }
        
        if (string.IsNullOrWhiteSpace(humanName.Family))
        {
            return GetSuccessfulResponse();
        }
        
        if (humanName.Given is not null && humanName.Given.Any() && humanName.Given.Any(string.IsNullOrWhiteSpace))
        {
            return GetSuccessfulResponse();
        }

        return GetInvalidResponse(message: "Patient.name au-core-pat-04: At least text, family name, or given name shall be present");
    }

    
    
}