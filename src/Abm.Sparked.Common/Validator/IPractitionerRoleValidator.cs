using FhirNavigator;
using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.Validator;

public interface IPractitionerRoleValidator
{
    Task<ValidatorResponse> Validate(PractitionerRole practitionerRole, IFhirNavigator? fhirNavigator = null);
}