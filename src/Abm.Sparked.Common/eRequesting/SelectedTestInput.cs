using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.eRequesting;

public record SelectedTestInput(string displayPrefix, CodeableConcept CodeableConcept);