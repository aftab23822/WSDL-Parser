using System.Text;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;

public class WsdlBodyGenerator
{
    private bool _ElementFound = false;
    private XmlSchemas _XmlSchemas;
    public string GenerateXmlFromWSDLSchema(XmlSchemas xmlSchemas, string elementName = null, string namespaceUri = null)
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
            WriteSoapEnvelopeAndBody(writer, namespaceUri, () => WriteBody(writer, elementName, namespaceUri));
        }

        return xmlBuilder.ToString();
    }

    private void WriteBody(XmlWriter writer, string elementName, string namespaceUri)
    {
        _ElementFound = false;
        HashSet<XmlQualifiedName> visitedTypes = new HashSet<XmlQualifiedName>();
        foreach (XmlSchema xmlSchema in _XmlSchemas)
            FindAndWriteElement(writer, xmlSchema.Items, elementName, namespaceUri, visitedTypes);

        WriteErrorIfElementNotFound(writer, elementName, namespaceUri);
    }

    private void FindAndWriteElement(XmlWriter writer, XmlSchemaObjectCollection schemaItems, string elementName, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        foreach (XmlSchemaObject schemaobj in schemaItems.OfType<XmlSchemaObject>())
        {
            if (schemaobj is XmlSchemaElement element && element.Name == elementName)
            {
                _ElementFound = true;
                WriteElementOrType(writer, element, schemaItems, namespaceUri, visitedTypes);
                break;
            }
            else if (schemaobj is XmlSchemaComplexType complexType && complexType.Name == elementName)
            {
                _ElementFound = true;
                WriteElementOrType(writer, complexType.Particle, schemaItems, namespaceUri, visitedTypes);
                break;
            }
        }
    }

    private void WriteErrorIfElementNotFound(XmlWriter writer, string elementName, string namespaceUri)
    {
        if (!_ElementFound)
        {
            writer.WriteStartElement("error", namespaceUri);
            writer.WriteValue($"The element or type could not be found {elementName}");
            writer.WriteEndElement();
        }
    }

    private void WriteSoapEnvelopeAndBody(XmlWriter writer, string namespaceUri, Action writeBodyAction)
    {
        writer.WriteStartElement("soap", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
        writer.WriteAttributeString("xmlns", "soap", null, "http://schemas.xmlsoap.org/soap/envelope/");
        if (!string.IsNullOrEmpty(namespaceUri))
            writer.WriteAttributeString("xmlns", "tns", null, namespaceUri);

        writer.WriteStartElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");

        writeBodyAction?.Invoke();

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private void WriteElementOrType(XmlWriter writer, XmlSchemaParticle particle, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        if (particle is XmlSchemaElement element)
            WriteElement(writer, element, schemaItems, namespaceUri, visitedTypes);

        else if (particle is XmlSchemaSequence sequence)
            WriteSequence(writer, sequence, schemaItems, namespaceUri, visitedTypes);

        else if (particle is XmlSchemaChoice choice)
            WriteChoice(writer, choice, schemaItems, namespaceUri, visitedTypes);

        else if (particle is XmlSchemaAll all)
            WriteAll(writer, all, schemaItems, namespaceUri, visitedTypes);
    }

    private void WriteElement(XmlWriter writer, XmlSchemaElement element, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        WriteStartElement(writer, element.Name, namespaceUri);

        HandleSchemaComplexType(writer, element, schemaItems, namespaceUri, visitedTypes);

        HandleSimpleType(writer, element);
        writer.WriteEndElement();
    }

    private void HandleSchemaComplexType(XmlWriter writer, XmlSchemaElement element, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        if (element.SchemaType != null)
            WriteComplexElementOrType(writer, element, schemaItems, namespaceUri, visitedTypes);

        else if (element.SchemaTypeName != null)
            WriteComplexType(writer, element.SchemaTypeName, schemaItems, namespaceUri, visitedTypes);
    }

    private void HandleSimpleType(XmlWriter writer, XmlSchemaElement element)
    {
        if (element is XmlSchemaElement)
            WriteSimpleElement(writer, element);
    }

    private void WriteSimpleElement(XmlWriter writer, XmlSchemaElement element)
    {
        if (element.DefaultValue != null || XmlSchemaHelper.IsSimpleType(element))
            writer.WriteValue(element.DefaultValue ?? SampleGenerator.GetSampleValue(element.SchemaTypeName.Name));
    }

    private void WriteComplexElementOrType(XmlWriter writer, XmlSchemaElement element, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        if (element.SchemaType is XmlSchemaComplexType complexType)
            WriteElementOrType(writer, complexType.Particle, schemaItems, namespaceUri, visitedTypes);
    }

    private void WriteStartElement(XmlWriter writer, string? elementName, string namespaceUri)
    {
        if (namespaceUri != null)
            writer.WriteStartElement("tns", elementName, namespaceUri);
        else
            writer.WriteStartElement(elementName);
    }

    private void WriteComplexType(XmlWriter writer, XmlQualifiedName complexTypeName, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        XmlQualifiedName currentType = new XmlQualifiedName(complexTypeName.Name, complexTypeName.Namespace);

        if (visitedTypes.Contains(currentType))
            return;

        HandleComplexType(writer, currentType, complexTypeName, schemaItems, namespaceUri, visitedTypes);
    }

    private void HandleComplexType(XmlWriter writer, XmlQualifiedName currentType, XmlQualifiedName complexTypeName, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        visitedTypes.Add(currentType);

        XmlSchemaComplexType complexType = schemaItems.OfType<XmlSchemaComplexType>().FirstOrDefault(t => t.QualifiedName == complexTypeName);

        if (complexType != null && complexType.Particle != null)
            WriteElementOrType(writer, complexType.Particle, schemaItems, namespaceUri, visitedTypes);

        visitedTypes.Remove(currentType);
    }

    private void WriteSequence(XmlWriter writer, XmlSchemaSequence sequence, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        foreach (XmlSchemaParticle childParticle in sequence.Items)
            WriteElementOrType(writer, childParticle, schemaItems, namespaceUri, visitedTypes);
    }

    private void WriteChoice(XmlWriter writer, XmlSchemaChoice choice, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        foreach (XmlSchemaParticle childParticle in choice.Items)
            WriteElementOrType(writer, childParticle, schemaItems, namespaceUri, visitedTypes);
    }

    private void WriteAll(XmlWriter writer, XmlSchemaAll all, XmlSchemaObjectCollection schemaItems, string namespaceUri, HashSet<XmlQualifiedName> visitedTypes)
    {
        foreach (XmlSchemaParticle childParticle in all.Items)
            WriteElementOrType(writer, childParticle, schemaItems, namespaceUri, visitedTypes);
    }

}
