using Hl7.Fhir.Model;
using Task = Hl7.Fhir.Model.Task;

namespace Abm.Sparked.Common.eRequesting;

public static class PathologyTaskFactory
{
    public static Task GetTask(PathologyTaskInput input)
    {
        var task = new Task();
        task.Id = input.ResourceId;
        task.GroupIdentifier = input.GroupIdentifier;
        task.Status = input.RequestStatus;
        task.Intent = input.Intent;
        task.Code = input.Code;
        task.AuthoredOnElement = new FhirDateTime(input.AuthoredOn);
        task.Focus = input.Focus;
        task.For = input.For;
        task.Owner = input.Owner;
        task.Requester = input.Requester;
        return task;
    }
}