using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.Validator;

public interface IPatientValidator
{
    ValidatorResponse Validate(Patient patient);
}