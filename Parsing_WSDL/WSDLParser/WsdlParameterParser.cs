using System;
using System.IO;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

internal class WsdlParameterGenerator
{
    private static XmlSchemas xmlSchemas;

    public static string GenerateXmlFromWSDLSchema(XmlSchemas schemas, Message message , string namespaceUri = null)
    {
        xmlSchemas = schemas;
        using (StringWriter stringWriter = new StringWriter())
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ", 
                NewLineChars = "\n", 
                NewLineHandling = NewLineHandling.Replace,
            };

            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                // Start writing the XML
                xmlWriter.WriteStartElement("soap", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
                xmlWriter.WriteStartElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
                xmlWriter.WriteStartElement("", message.Name, "");

                foreach (MessagePart part in message.Parts)
                {
                    if (!XmlSchemaHelper.IsKnownSimpleType(part.Type.Name))
                    {
                        xmlWriter.WriteStartElement("", message.Name, "");

                        var xml = WsdlBodyGenerator.GenerateXmlFromWSDLSchema(xmlSchemas, part.Type.Name);
                        XmlDocument generatedXmlDoc = new XmlDocument();
                        generatedXmlDoc.LoadXml(xml);

                        foreach (XmlNode childNode in generatedXmlDoc.DocumentElement?.ChildNodes[0]?.ChildNodes)
                        {
                            childNode.WriteTo(xmlWriter);
                        }

                        xmlWriter.WriteEndElement();
                    }
                    else
                    {
                        xmlWriter.WriteStartElement("", part.Name, "");
                        xmlWriter.WriteString(SampleGenerator.GetSampleValue(part.Type.Name));
                        xmlWriter.WriteEndElement();
                    }
                }

                xmlWriter.WriteEndElement(); 
                xmlWriter.WriteEndElement(); // Close soap:Body element
                xmlWriter.WriteEndElement(); // Close soap:Envelope element
            }

            return stringWriter.ToString();
        }
    }
}
