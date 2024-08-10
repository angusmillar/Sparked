using Abm.Sparked.Common.Constants;
using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.Support;

public static class CodeableConceptSupport
{
    public static CodeableConcept GetLaboratoryProcedure()
    {
        return new CodeableConcept()
        {
            Coding = new List<Coding>()
            {
                CodeingSupport.GetSnomed(code: "108252007", display: "Laboratory procedure")
            },
            Text = "Laboratory procedure"
        };
    }
    
    
    public static CodeableConcept GetRequestedSnomedTest(string term, string preferredDisplay, string? text = null)
    {
        return new CodeableConcept()
        {
            Coding = new List<Coding>()
            {
                CodeingSupport.GetSnomed(code: term, display: preferredDisplay)
            },
            Text = text
        };
    }
    
    public static CodeableConcept GetRequestedFreeTextTest(string freeTextTest)
    {
        return new CodeableConcept()
        {
            Text = freeTextTest
        };
    }
    
}

