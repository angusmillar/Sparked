using Abm.Sparked.Common.Constants;
using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.Support;

public static class CodeingSupport
{
    public static Coding GetSnomed(string code, string? display)
    {
        return new Coding(
            system: CodeSystemsConstants.SnomedCtSystem, 
            code: code, 
            display: display);
    }
    
}

