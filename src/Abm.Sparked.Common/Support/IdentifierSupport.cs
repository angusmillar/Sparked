using Abm.Sparked.Common.Constants;
using Hl7.Fhir.Model;

namespace Abm.Sparked.Common.Support;

public static class IdentifierSupport
{
    public static Identifier GetHpio(string hpioValue)
    {
        return new Identifier(system: IdentifiersConstants.HpioSystem, value: hpioValue)
        {
            Type = new CodeableConcept(
                system: IdentifiersConstants.HpioTypeSystem, 
                code: IdentifiersConstants.HpioTypeCode,
                text: IdentifiersConstants.HpioTypeText)
        };
    }
    
    public static Identifier GetRequisition(string scopeingHpio, string value)
    {
        return new Identifier(system: Path.Combine(IdentifiersConstants.OrderIdentifierHpioScopedSystem, RemoveWhitespace(scopeingHpio)), value: value)
        {
            Type = new CodeableConcept(
                system: IdentifiersConstants.V2Table0203System, 
                code: IdentifiersConstants.PlacerGroupNumberTypeCode)
        };
    }
    
    public static string RemoveWhitespace(this string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !Char.IsWhiteSpace(c))
            .ToArray());
    }
}

