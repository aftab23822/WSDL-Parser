using System.Xml.Schema;

class XmlSchemaHelper
{
    // Function to check if an XmlSchemaElement represents a simple type like string, int, etc.
    public static bool IsSimpleType(XmlSchemaElement element)
    {
        if (element.SchemaType != null)
        {
            if (element.SchemaType is XmlSchemaComplexType complexType)
            {
                // Check if the complex type has no particle (no nested elements)
                return complexType.Particle == null;
            }
        }
        else if (element.SchemaTypeName != null)
        {
            // Check if the SchemaTypeName.Name represents a simple type like string, int, etc.
            string typeName = element.SchemaTypeName.Name;

            // Your logic to determine if typeName is a known simple type goes here
            if (IsKnownSimpleType(typeName))
            {
                return true;
            }
        }

        return false;  // Assume all other cases are not a simple type
    }

    // Helper function to check if typeName represents a known simple type
    public static bool IsKnownSimpleType(string typeName)
    {
        // List of commonly used XML schema simple types
        switch (typeName.ToLower())
        {
            case "string":
            case "boolean":
            case "decimal":
            case "float":
            case "double":
            case "duration":
            case "dateTime":
            case "time":
            case "date":
            case "gyear":
            case "gyearmonth":
            case "gmonth":
            case "gmonthday":
            case "gday":
            case "hexbinary":
            case "base64binary":
            case "anyuri":
            case "qname":
            case "notation":
            case "normalizedstring":
            case "token":
            case "language":
            case "nmtoken":
            case "nmtokens":
            case "name":
            case "ncname":
            case "id":
            case "idref":
            case "idrefs":
            case "entity":
            case "entities":
            case "integer":
            case "nonpositiveinteger":
            case "negativeinteger":
            case "long":
            case "int":
            case "short":
            case "byte":
            case "nonnegativeinteger":
            case "unsignedlong":
            case "unsignedint":
            case "unsignedshort":
            case "unsignedbyte":
            case "positiveinteger":
            case "datetime":
                return true;
            default:
                return false;
        }
    }

}
