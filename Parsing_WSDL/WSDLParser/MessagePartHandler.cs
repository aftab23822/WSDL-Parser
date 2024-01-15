
using System.Web.Services.Description;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Parsing_WSDL.WSDLParser
{

    public class MessagePartHandler
    {
        private ServiceDescriptionCollection _services = new();
        private XmlSchemas _XmlSchemas { get; set; }
        public MessagePartHandler(ServiceDescriptionCollection services)
        {
            _services = services;
            _XmlSchemas = _services[0].Types.Schemas;
        }

        public void HanldeInputOutputBody(Operation operation, OperationWSDL operationWsdl)
        {
            var messageinput = GetMessage(operation.Messages.Input);
            var messageoutput = GetMessage(operation.Messages.Output);
            var messagePartHandler = new MessagePartHandler(_services);
            messagePartHandler.SetOperationWsdlInput(operationWsdl, messageinput);
            messagePartHandler.SetOperationWsdlOutput(operationWsdl, messageoutput);
        }


        private Message GetMessage(OperationMessage operationMsg)
        {
            var messages = _services[0].Messages;
            return operationMsg != null ? messages[operationMsg.Message.Name] : null;
        }

        private void SetOperationWsdlInput(OperationWSDL operationWsdl, Message messageinput)
        {
            foreach (MessagePart messagePart in messageinput.Parts)
                HandleInputMessagePart(messagePart, operationWsdl, messageinput);
        }

        private void HandleInputMessagePart(MessagePart messagePart, OperationWSDL operationWsdl, Message messageinput)
        {
            if (string.IsNullOrEmpty(messagePart.Element.Name))
                HandleInputParameters(operationWsdl, messageinput, messagePart);
            else
                HandleInputBody(operationWsdl, messagePart);
        }

        private void HandleInputBody(OperationWSDL operationWsdl, MessagePart messagePart)
        {
            operationWsdl.input = WsdlBodyGenerator.GenerateXmlFromWSDLSchema(_XmlSchemas, messagePart.Element.Name, _services[0].TargetNamespace);
            if (!string.IsNullOrEmpty(operationWsdl.input)) return;
            var type = GetMessagePartType(messagePart.Element.Name);
            operationWsdl.input = "<" + type + ">" + messagePart.Element.Name + "<" + type + ">";
        }

        private void HandleInputParameters(OperationWSDL operationWsdl, Message messageinput, MessagePart messagePart)
        {
            if (operationWsdl.HttpVerb.Trim().ToUpper() == ApiConstants.GET || operationWsdl.InputContentType.ToLower() == ApiConstants.ApplicationFormContentType || operationWsdl.InputContentType.ToLower() == ApiConstants.MultipartContentType)
                operationWsdl.InputParameters.Add(new(messagePart.Name, messagePart.Type.Name, SampleGenerator.GetSampleValue(messagePart.Type.Name), operationWsdl.SendInputBodyAs));
            else
                operationWsdl.input = WsdlParameterGenerator.GenerateXmlFromWSDLSchema(_XmlSchemas, messageinput, messagePart.Type.Namespace);
        }

        private string GetMessagePartType(string elementName)
        {
            foreach (XmlSchema xmlSchema in _XmlSchemas)
            {
                var schemaItems = xmlSchema.Items;
                var element = schemaItems.OfType<XmlSchemaElement>().FirstOrDefault(e => e.Name == elementName);
                if (element == null) return null;
                return string.IsNullOrEmpty(element.SchemaTypeName.Name) ? null : element.SchemaTypeName.Name;
            }
            return null;
        }


        private void SetOperationWsdlOutput(OperationWSDL operationWsdl, Message messageoutput)
        {
            foreach (MessagePart messagePart in messageoutput.Parts)
                HandleOutputMessagePart(operationWsdl, messageoutput, messagePart);
        }

        private void HandleOutputMessagePart(OperationWSDL operationWsdl, Message messageoutput, MessagePart messagePart)
        {
            if (string.IsNullOrEmpty(messagePart.Element.Name))
                HandleOutputParameters(operationWsdl, messageoutput, messagePart);
            else
                HandleOutputBody(operationWsdl, messagePart);
        }

        private void HandleOutputBody(OperationWSDL operationWsdl, MessagePart messagePart)
        {
            operationWsdl.output = WsdlBodyGenerator.GenerateXmlFromWSDLSchema(_XmlSchemas, messagePart.Element.Name, _services[0].TargetNamespace);
            if (!string.IsNullOrEmpty(operationWsdl.output)) return;
            var type = GetMessagePartType(messagePart.Element.Name);
            operationWsdl.output = "<" + type + ">" + messagePart.Element.Name + "<" + type + ">";
        }

        private void HandleOutputParameters(OperationWSDL operationWsdl, Message messageoutput, MessagePart messagePart)
        {
            if (operationWsdl.HttpVerb.Trim().ToUpper() == ApiConstants.GET || operationWsdl.InputContentType.ToLower() == ApiConstants.ApplicationFormContentType || operationWsdl.InputContentType.ToLower() == ApiConstants.MultipartContentType)
                operationWsdl.OutputParameters.Add(new(messagePart.Name, messagePart.Type.Name, SampleGenerator.GetSampleValue(messagePart.Type.Name), operationWsdl.SendInputBodyAs));
            else
                operationWsdl.output = WsdlParameterGenerator.GenerateXmlFromWSDLSchema(_XmlSchemas, messageoutput, messagePart.Type.Namespace);
        }

    }
}
