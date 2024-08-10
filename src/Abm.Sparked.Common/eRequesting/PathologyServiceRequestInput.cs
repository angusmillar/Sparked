using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.eRequesting;

public record PathologyServiceRequestInput(
    string ResourceId,
    Identifier Requisition,
    CodeableConcept RequestedTest,
    DateTimeOffset requestedDateTime,
    ResourceReference PatientReference,
    ResourceReference RequesterPractitionerRoleReference);