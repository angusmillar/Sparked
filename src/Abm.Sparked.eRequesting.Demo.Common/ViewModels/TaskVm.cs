using Hl7.Fhir.Utility;
using Task = Hl7.Fhir.Model.Task;

namespace Abm.Sparked.eRequesting.Demo.Common.ViewModels;

public class TaskVm
{
    public string Id { get; set; } = string.Empty;

    public IdentifierVm GroupIdentifier { get; set; } = new IdentifierVm();
    public string Status { get; set; } = string.Empty;

    public string StatusReason { get; set; } = string.Empty;

    public string BusinessStatus { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;

    public string FocusReferenceValue { get; set; } = string.Empty;

    public string ForReferenceValue { get; set; } = string.Empty;

    public string OwnerReferenceValue { get; set; } = string.Empty;

    public DateTime LastModified { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public DateTime AuthoredOn { get; set; } = DateTime.Now;

    public static TaskVm CreateViewModel(Task fhirResource)
    {
        DateTime lastUpdated = DateTime.MinValue;
        if (fhirResource.Meta.LastUpdated.HasValue)
        {
            lastUpdated = fhirResource.Meta.LastUpdated.Value.DateTime;
        }

        return new TaskVm
        {
            Id = fhirResource.Id,
            Intent = fhirResource.Intent!.Value.GetLiteral(),
            AuthoredOn = fhirResource.AuthoredOnElement.ToDateTimeOffset(TimeSpan.FromHours(10)).DateTime,
            LastUpdated = lastUpdated
        };

    }
}