using System.Linq.Expressions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;

namespace Abm.Sparked.eRequesting.Demo.Common.ViewModels;

public class ServiceRequestVm
{
    public string Id { get; set; } = string.Empty;
    public string TestRequested { get; set; } = string.Empty;
    public IdentifierVm Requisition { get; set; } = new IdentifierVm();
    public string Status { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    public DateTime AuthoredOn { get; set; } = DateTime.Now;
    public static ServiceRequestVm CreateViewModel(ServiceRequest fhirResource)
    {
        string? testRequested = fhirResource.Code?.Coding?.FirstOrDefault()?.Display;
        if (testRequested is null)
        {
            testRequested = fhirResource.Code?.Text ?? "No Test Found";
        }
        
        return new ServiceRequestVm
        {
            Id = fhirResource.Id,
            TestRequested = testRequested,
            Requisition = new IdentifierVm()
            {
                Type = fhirResource.Requisition.Type.Coding.First().Code,
                System = fhirResource.Requisition.System,
                Value = fhirResource.Requisition.Value
            },
            Status = fhirResource.Status!.Value.GetLiteral(),
            Intent = fhirResource.Intent!.Value.GetLiteral(),
            Category = fhirResource.Category.First().Coding.First().Display,
        };
    }
    // public static Expression<Func<ServiceRequest, ServiceRequestVm>> FromFhir
    // {
    //     get
    //     {
    //         return fhirResource => new ServiceRequestVm
    //         {
    //             Id = fhirResource.Id,
    //             TestRequested = "asd",
    //             Requisition = new IdentifierVm()
    //             {
    //                 Type = fhirResource.Requisition.Type.Coding.First().Code,
    //                 System = fhirResource.Requisition.System,
    //                 Value = fhirResource.Requisition.Value
    //             },
    //             Status = fhirResource.Status!.Value.GetLiteral(),
    //             Intent = fhirResource.Intent!.Value.GetLiteral()
    //         };
    //     }
    // }
}