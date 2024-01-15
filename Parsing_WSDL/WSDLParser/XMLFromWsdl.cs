using System.Text;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

public class WsdlBodyGenerator
{
    private static XmlSchemas _XmlSchemas;
    public static string GenerateXmlFromWSDLSchema(XmlSchemas xmlSchemas, string elementName = null, string namespaceUri = null)
    {
        _XmlSchemas = xmlSchemas;
        StringBuilder xmlBuilder = new StringBuilder();
        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false
        };

        using (XmlWriter writer = XmlWriter.Create(xmlBuilder, settings))
        {
            WriteSoapEnvelopeAndBody(writer, namespaceUri, () =>
            {
                var found = false;
                HashSet<XmlQualifiedName> visitedTypes = new HashSet<XmlQualifiedName>();

                foreach (XmlSchema xmlSchema in _XmlSchemas)
                {
                    var schemaItems = xmlSchema.Items;
                    foreach (XmlSchemaObject schemaobj in schemaItems.OfType<XmlSchemaObject>())
                    {
                        if (schemaobj is XmlSchemaElement element && element.Name == elementName)
                        {
                            found = true;
                            WriteElementOrType(writer, element, schemaItems, namespaceUri, visitedTypes);
                            break;
                        }
                        if(schemaobj is XmlSchemaComplexType complexType && complexType.Name == elementName)
                        {
                            found= true;
                            WriteElementOrType(writer, complexType.Particle, schemaItems, namespaceUri, visitedTypes);
                            break;
                        }
                    }
                    
                }
                if (!found)
                {
                    writer.WriteStartElement("error", namespaceUri);
                    writer.WriteValue($"The element or type could not be found {elementName}");
                    writer.WriteEndElement();
                }
            });
        }

        return xmlBuilder.ToString();
    }

    private static void WriteSoapEnvelopeAndBody(XmlWriter writer, string namespaceUri, Action writeBodyAction)
    {
        writer.WriteStartElement("soap", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
        writer.WriteAttributeString("xmlns", "soap", null, "http://schemas.xmlsoap.org/soap/envelope/");
        if (!string.IsNullOrEmpty(namespaceUri))
        {
            writer.WriteAttributeString("xmlns", "tns", null, namespaceUri);
        }

        writer.WriteStartElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");

        writeBodyAction?.Invoke();

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteElementOrType(XmlWriter writer, XmlSchemaParticle particle, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        if (particle is XmlSchemaElement element)
        {
            WriteElement(writer, element, schemaItems, namespaceUri, visitedTypes);
        }
        else if (particle is XmlSchemaSequence sequence)
        {
            WriteSequence(writer, sequence, schemaItems, namespaceUri, visitedTypes);
        }
        else if (particle is XmlSchemaChoice choice)
        {
            WriteChoice(writer, choice, schemaItems, namespaceUri, visitedTypes);
        }
        else if (particle is XmlSchemaAll all)
        {
            WriteAll(writer, all, schemaItems, namespaceUri, visitedTypes);
        }
    }

    private static void WriteElement(XmlWriter writer, XmlSchemaElement element, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        if(namespaceUri!= null) 
        { 
            writer.WriteStartElement("tns", element.Name, namespaceUri);
        }
        else
        {
            writer.WriteStartElement(element.Name);
        }

        if (element.SchemaType != null)
        {
            if (element.SchemaType is XmlSchemaComplexType complexType)
            {
                WriteElementOrType(writer, complexType.Particle, schemaItems, namespaceUri, visitedTypes);
            }
        }
        else if (element.SchemaTypeName != null)
        {
            WriteComplexType(writer, element.SchemaTypeName, schemaItems, namespaceUri, visitedTypes);
        }
        if (element is XmlSchemaElement)
        {
            if (element.DefaultValue != null || XmlSchemaHelper.IsSimpleType(element))
            {
                writer.WriteValue(element.DefaultValue ?? SampleGenerator.GetSampleValue(element.SchemaTypeName.Name));
            }
        }
        writer.WriteEndElement();
    }

    private static void WriteComplexType(XmlWriter writer, XmlQualifiedName complexTypeName, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        XmlQualifiedName currentType = new XmlQualifiedName(complexTypeName.Name, complexTypeName.Namespace);

        if (visitedTypes.Contains(currentType))
        {
            return;
        }

        visitedTypes.Add(currentType);

        XmlSchemaComplexType complexType = schemaItems.OfType<XmlSchemaComplexType>()
            .FirstOrDefault(t => t.QualifiedName == complexTypeName);

        if (complexType != null && complexType.Particle != null)
        {
            WriteElementOrType(writer, complexType.Particle, schemaItems, namespaceUri, visitedTypes);
        }

        visitedTypes.Remove(currentType);
    }

    private static void WriteSequence(XmlWriter writer, XmlSchemaSequence sequence, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        foreach (XmlSchemaParticle childParticle in sequence.Items)
        {
            WriteElementOrType(writer, childParticle, schemaItems, namespaceUri, visitedTypes);
        }
    }

    private static void WriteChoice(XmlWriter writer, XmlSchemaChoice choice, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        foreach (XmlSchemaParticle childParticle in choice.Items)
        {
            WriteElementOrType(writer, childParticle, schemaItems, namespaceUri, visitedTypes);
        }
    }

    private static void WriteAll(XmlWriter writer, XmlSchemaAll all, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        foreach (XmlSchemaParticle childParticle in all.Items)
        {
            WriteElementOrType(writer, childParticle, schemaItems, namespaceUri, visitedTypes);
        }
    }

}
