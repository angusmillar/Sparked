namespace Abm.Sparked.Common.Validator;

public record ValidatorResponse(bool IsValid, string? Message = null);