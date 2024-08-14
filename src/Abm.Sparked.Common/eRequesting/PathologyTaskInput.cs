using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.eRequesting;

public record PathologyTaskInput(
    string ResourceId,
    Identifier GroupIdentifier,
    Hl7.Fhir.Model.Task.TaskStatus RequestStatus,
    Hl7.Fhir.Model.Task.TaskIntent Intent,
    CodeableConcept Code,
    DateTimeOffset AuthoredOn,
    ResourceReference Focus,
    ResourceReference Owner,
    ResourceReference Requester);