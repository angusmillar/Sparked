using FhirNavigator;
using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.Validator;

public interface IOrganizationRoleValidator
{
    Task<ValidatorResponse> Validate(Organization organization, IFhirNavigator? fhirNavigator = null);
}