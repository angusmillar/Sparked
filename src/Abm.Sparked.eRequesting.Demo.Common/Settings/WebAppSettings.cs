
namespace Abm.Sparked.eRequesting.Demo.Common.Settings;

public sealed class WebAppSettings
{
    public const string SectionName = "Settings";
    
    public string DefaultFhirRepositoryCode { get; init; } = "[Has not been set by the application]";
    public string FillerOrganizationResourceId { get; init; } = "[Has not been set by the application]";
    
}