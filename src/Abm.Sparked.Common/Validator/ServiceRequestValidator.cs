using FhirNavigator;
using Abm.Sparked.Common.Support;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;

namespace Abm.Sparked.Common.Validator;

public class ServiceRequestValidator(IPatientValidator patientValidator) :ValidatorBase, IServiceRequestValidator
{
    private IFhirNavigator? _fhirNavigator;
    
    /// <summary>
    /// Will validate against the Sparked eRequesting rules
    /// If IFhirNavigator provided the validation will resolve resource references and validate those resources as well. 
    /// </summary>
    /// <param name="serviceRequest"></param>
    /// <param name="fhirNavigator"></param>
    /// <returns></returns>
    public async Task<ValidatorResponse> Validate(ServiceRequest serviceRequest, IFhirNavigator? fhirNavigator = null)
    {
        _fhirNavigator = fhirNavigator;
        
        var validatorResponseList = new List<ValidatorResponse>();

        validatorResponseList.Add(ValidateRequisition(serviceRequest.Requisition));
        validatorResponseList.Add(ValidateStatus(serviceRequest.Status));
        validatorResponseList.Add(ValidateIntent(serviceRequest.Intent));
        validatorResponseList.Add(ValidateCategory(serviceRequest.Category));
        validatorResponseList.Add(ValidateCode(serviceRequest.Code));
        validatorResponseList.Add(ValidateReferencePopulated(serviceRequest.Subject, "ServiceRequest.subject"));
        validatorResponseList.Add(ValidateAuthoredOn(serviceRequest.AuthoredOnElement));
        validatorResponseList.Add(ValidateReferencePopulated(serviceRequest.Requester ,"ServiceRequest.Requester"));
        
        validatorResponseList.Add(await ValidateSubjectReference(serviceRequest));
        
        return ConsolidatedValidationResponse(validatorResponseList);
    }
    
    private async Task<ValidatorResponse> ValidateSubjectReference(ServiceRequest serviceRequest)
    {
        if (serviceRequest.Subject is null)
        {
            return GetInvalidResponse(message: "ServiceRequest.subject SHALL NOT be empty");
        }
        
        if (string.IsNullOrWhiteSpace(serviceRequest.Subject.Reference))
        {
            return GetInvalidResponse(message: "ServiceRequest.subject SHALL NOT be empty");
        }
        
        if (_fhirNavigator is null)
        {
            return GetSuccessfulResponse();
        }
        
        Patient? patient = await _fhirNavigator.GetResource<Patient>(serviceRequest.Subject, "ServiceRequest.subject", serviceRequest);
        if (patient is null)
        {
            return GetInvalidResponse(message: "ServiceRequest.subject unable to resolve the Patient resource from the default FHIR Repository");
        }
        
        ValidatorResponse patientValidatorResponse = patientValidator.Validate(patient);
        if (!patientValidatorResponse.IsValid)
        {
            return patientValidatorResponse;
        }

        return GetSuccessfulResponse();
    }

    private ValidatorResponse ValidateRequisition(Identifier? identifier)
    {
        if (identifier is null)
        {
            return GetInvalidResponse(message: "ServiceRequest.requisition SHALL NOT be empty");    
        }
        return GetSuccessfulResponse();
    }
    private ValidatorResponse ValidateStatus(RequestStatus? status)
    {
        if (status is null)
        {
            return GetInvalidResponse(message: "ServiceRequest.status SHALL NOT be empty");    
        }
        
        if (!status.Equals(RequestStatus.Active))
        {
            return GetInvalidResponse(message: $"ServiceRequest.status SHALL be: {RequestStatus.Active.GetLiteral()}");    
        }
        
        return GetSuccessfulResponse();
    }
    private ValidatorResponse ValidateIntent(RequestIntent? requestIntent)
    {
        if (requestIntent is null)
        {
            return GetInvalidResponse(message: "ServiceRequest.intent SHALL NOT be empty");    
        }
        
        if (!requestIntent.Equals(RequestIntent.Order))
        {
            return GetInvalidResponse(message: $"ServiceRequest.intent SHALL be: {RequestIntent.Order.GetLiteral()}");    
        }
        
        return GetSuccessfulResponse();
    }
    private ValidatorResponse ValidateCategory(List<CodeableConcept> categoryList)
    {
        
        if (!categoryList.Any())
        {
            return GetInvalidResponse(message: $"ServiceRequest.category SHALL NOT be empty");    
        }

        var containsLaboratoryProcedureCodeResponse = ContainsLaboratoryProcedureCode(categoryList);
        if (!containsLaboratoryProcedureCodeResponse.IsValid)
        {
            return containsLaboratoryProcedureCodeResponse;
        }
        
        return GetSuccessfulResponse();
    }
    private ValidatorResponse ValidateCode(CodeableConcept? serviceRequestCode)
    {
        if (serviceRequestCode is null)
        {
            return GetInvalidResponse(message: "ServiceRequest.code SHALL NOT be empty");    
        }

        if (string.IsNullOrWhiteSpace(serviceRequestCode.Text) && !serviceRequestCode.Coding.Any())
        {
            return GetInvalidResponse(
                message: "ServiceRequest.code.text and ServiceRequest.code.coding SHALL NOT both be empty");
        }
        
        return GetSuccessfulResponse();
    }
    
    private ValidatorResponse ValidateAuthoredOn(FhirDateTime serviceRequestAuthoredOn)
    {
        if (string.IsNullOrWhiteSpace(serviceRequestAuthoredOn.Value))
        {
            return GetInvalidResponse(message: "ServiceRequest.authoredOn SHALL NOT be empty");    
        }

        try
        {
            serviceRequestAuthoredOn.ToDateTime();
        }
        catch (FormatException)
        {
            return GetInvalidResponse(message: $"ServiceRequest.authoredOn was found to be an invalid format: {serviceRequestAuthoredOn.Value}");    
        }
        
        return GetSuccessfulResponse();
    }
    private ValidatorResponse ContainsLaboratoryProcedureCode(List<CodeableConcept> categoryList)
    {
        CodeableConcept laboratoryProcedure = CodeableConceptSupport.GetLaboratoryProcedure();
        string laboratoryProcedureCode = laboratoryProcedure.Coding.First().Code;
        string laboratoryProcedureSystem = laboratoryProcedure.Coding.First().System;
        
        var isLaboratoryProcedure = categoryList.Any(x =>
            x.Coding.Any(c => c.Code.Equals(laboratoryProcedureCode, StringComparison.OrdinalIgnoreCase) && c.System.Equals(laboratoryProcedureSystem, StringComparison.OrdinalIgnoreCase)));
        
        if (!isLaboratoryProcedure)
        {
            GetInvalidResponse(
                $"ServiceRequest.category SHALL contain the code: {laboratoryProcedureCode} and system: {laboratoryProcedureSystem}");
        }
        
        return GetSuccessfulResponse();
    }
    
   
}