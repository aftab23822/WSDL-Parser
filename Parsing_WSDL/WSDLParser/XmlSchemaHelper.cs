using System.Xml.Schema;

class XmlSchemaHelper
{
    public static bool IsSimpleType(XmlSchemaElement element)
    {
        if (element.SchemaType != null)
        {
            if (element.SchemaType is XmlSchemaComplexType complexType)
                return complexType.Particle == null;
        }
        else if (element.SchemaTypeName != null)
        {
            string typeName = element.SchemaTypeName.Name;
            if (IsKnownSimpleType(typeName))
                return true;
        }

        return false;
    }


    public static bool IsKnownSimpleType(string typeName)
    {
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
