using FhirNavigator;
using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.Validator;

public interface IServiceRequestValidator
{
    /// <summary>
    /// Will validate against the Sparked eRequesting rules
    /// If IFhirNavigator provided the validation will resolve resource references and validate those resources as well. 
    /// </summary>
    /// <param name="serviceRequest"></param>
    /// <param name="fhirNavigator"></param>
    /// <returns></returns>
    Task<ValidatorResponse> Validate(ServiceRequest serviceRequest, IFhirNavigator? fhirNavigator = null);
}