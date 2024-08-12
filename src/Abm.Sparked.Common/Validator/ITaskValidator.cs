using FhirNavigator;

namespace Abm.Sparked.Common.Validator;

public interface ITaskValidator
{
    Task<ValidatorResponse> Validate(Hl7.Fhir.Model.Task task, IFhirNavigator? fhirNavigator = null);
}