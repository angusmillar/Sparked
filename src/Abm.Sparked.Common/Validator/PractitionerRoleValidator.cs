using Abm.Sparked.Common.Constants;
using Abm.Sparked.Common.Support;
using FhirNavigator;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using Task = Hl7.Fhir.Model.Task;

namespace Abm.Sparked.Common.Validator;

public class PractitionerRoleValidator : ValidatorBase, IPractitionerRoleValidator
{
    
    private IFhirNavigator? _fhirNavigator;
    
    public async Task<ValidatorResponse> Validate(PractitionerRole practitionerRole, IFhirNavigator? fhirNavigator = null)
    {
        _fhirNavigator = fhirNavigator;
        await System.Threading.Tasks.Task.Delay(1);
        
        var validatorResponseList = new List<ValidatorResponse>();

        //validatorResponseList.Add(ValidateGroupIdentifier(practitionerRole.GroupIdentifier));
        
        return ConsolidatedValidationResponse(validatorResponseList);
        
    }

    
    
    
}