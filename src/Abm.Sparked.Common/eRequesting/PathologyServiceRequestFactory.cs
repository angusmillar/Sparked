using Abm.Sparked.Common.Support;
using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.eRequesting;

public static class PathologyServiceRequestFactory
{
    public static ServiceRequest GetServiceRequest(PathologyServiceRequestInput input)
    {
        var serviceRequestOne = new ServiceRequest();
        serviceRequestOne.Id = input.ResourceId;
        serviceRequestOne.Requisition = input.Requisition;
        serviceRequestOne.Status = RequestStatus.Active;
        serviceRequestOne.Intent = RequestIntent.Order;
        serviceRequestOne.Category = new List<CodeableConcept>() { CodeableConceptSupport.GetLaboratoryProcedure() };
        serviceRequestOne.Code = input.RequestedTest;
        serviceRequestOne.Subject = input.PatientReference;
        serviceRequestOne.AuthoredOnElement = new FhirDateTime(input.requestedDateTime);
        serviceRequestOne.Requester = input.RequesterPractitionerRoleReference;
        return serviceRequestOne;
    }
}