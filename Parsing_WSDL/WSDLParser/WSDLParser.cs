using System.Data;
using System.Web.Services.Description;
using System.Xml.Schema;
using System.Xml.Serialization;
using Parsing_WSDL.WSDLParser;

public class WSDLParser
{
    private ServiceDescriptionCollection _services = new();
    private XmlSchemas _XmlSchemas { get; set; }

    private bool _ParamArray;

    private List<WSDLServiceTypes> Parse(ServiceDescription serviceDescription)
    {
        _services.Add(serviceDescription);
        SetNamespace(serviceDescription);
        var services = new List<WSDLServiceTypes>();
        ParseServices(services);
        return services;
    }

    private void SetNamespace(ServiceDescription serviceDescription)
    {
        if (serviceDescription.Name == string.Empty) serviceDescription.Name = serviceDescription.RetrievalUrl;
        if (serviceDescription.Name == string.Empty) serviceDescription.Name = serviceDescription.TargetNamespace;
    }

    private void ParseServices(List<WSDLServiceTypes> services)
    {
        foreach (Service service in _services[0].Services)
        {
            var wsdlService = new WSDLServiceTypes();
            wsdlService.name = service.Name;
            wsdlService.port = new List<PortWSDL>();
            ParsePorts(service, wsdlService);
            services.Add(wsdlService);
        }
    }

    private void ParsePorts(Service service, WSDLServiceTypes wsd)
    {
        foreach (Port port in service.Ports)
        {
            var portWsdl = new PortWSDL();
            portWsdl.operations = new List<OperationWSDL>();

            var bindName = port.Binding;
            var binding = _services.GetBinding(bindName);
            var portType = _services.GetPortType(binding.Type);

            portWsdl.name = port.Name;
            portWsdl.address = GetAddress(port);

            ParseOperations(portWsdl, binding, portType);

            wsd.port.Add(portWsdl);
        }
    }

    private void ParseOperations(PortWSDL portWsdl, Binding binding, PortType portType)
    {
        foreach (OperationBinding operationbinding in binding.Operations)
            ParseMessages(operationbinding, portType, binding, portWsdl);
    }

    private void ParseMessages(OperationBinding operationbinding, PortType portType, Binding binding, PortWSDL portWsdl)
    {

        Operation operation = portType.Operations.Cast<Operation>().FirstOrDefault(x => x.Name == operationbinding.Name);
        OperationWSDL operationWsdl = new OperationWSDL();
        operationWsdl.HttpVerb = GetHttpVerbFromBinding(binding);
        operationWsdl.Description = operation?.Documentation.Trim();
        operationWsdl.name = operation?.Name;
        operationWsdl.actionUrl = GetActionUrl(portWsdl.address, operationbinding);
        SetInputAndContentType(operationWsdl, operationbinding);
        SetOutputAndContentType(operationWsdl, operationbinding);
        SetInputOutputBody(operation, operationWsdl);
        portWsdl.operations.Add(operationWsdl);
    }

    private void SetInputOutputBody(Operation operation, OperationWSDL operationWsdl)
    {
        _XmlSchemas = _services[0].Types.Schemas;
        var messageinput = GetMessage(operation.Messages.Input);
        var messageoutput = GetMessage(operation.Messages.Output);

        SetOperationWsdlInput(operationWsdl, messageinput);
        SetOperationWsdlOutput(operationWsdl, messageoutput);

    }

    private void SetOperationWsdlOutput(OperationWSDL operationWsdl, Message messageoutput)
    {
        foreach (MessagePart messagePart in messageoutput.Parts)
        {

            if (string.IsNullOrEmpty(messagePart.Element.Name)) //parameter
            {
                if (XmlSchemaHelper.IsKnownSimpleType(messagePart.Type.Name))
                    operationWsdl.OutputParameters.Add(new(messagePart.Name, messagePart.Type.Name, SampleGenerator.GetSampleValue(messagePart.Type.Name), operationWsdl.SendInputBodyAs));
                 else
                {

                    //operationWsdl.input = WsdlBodyGenerator.GenerateXmlFromWSDLSchema(_XmlSchemas, messagePart.Type.Name, _services[0].TargetNamespace);
                }
            }
            else
            {
                operationWsdl.output = WsdlBodyGenerator.GenerateXmlFromWSDLSchema(_XmlSchemas, messagePart.Element.Name, _services[0].TargetNamespace);
                if (!string.IsNullOrEmpty(operationWsdl.output)) return;
                var type = GetMessagePartType(messagePart.Element.Name);
                operationWsdl.output = "<" + type + ">" + messagePart.Element.Name + "<" + type + ">";
            }
        }
    }

    private void SetOperationWsdlInput(OperationWSDL operationWsdl, Message messageinput)
    {
        foreach (MessagePart messagePart in messageinput.Parts)
        {
            if (string.IsNullOrEmpty(messagePart.Element.Name)) //parameter
            {
                if (XmlSchemaHelper.IsKnownSimpleType(messagePart.Type.Name))
                    operationWsdl.InputParameters.Add(new(messagePart.Name, messagePart.Type.Name, SampleGenerator.GetSampleValue(messagePart.Type.Name), operationWsdl.SendInputBodyAs));
                else
                {

                    //operationWsdl.input = WsdlBodyGenerator.GenerateXmlFromWSDLSchema(_XmlSchemas, messagePart.Type.Name, _services[0].TargetNamespace);
                }               //operationWsdl.input = messageinput?.Parts[0].Name + "=" + messageinput?.Parts[0].Type.Name;
            }
            else
            {
                operationWsdl.input = WsdlBodyGenerator.GenerateXmlFromWSDLSchema(_XmlSchemas, messagePart.Element.Name, _services[0].TargetNamespace);
                if (!string.IsNullOrEmpty(operationWsdl.input)) return;
                var type = GetMessagePartType(messagePart.Element.Name);
                operationWsdl.input = "<" + type + ">" + messagePart.Element.Name + "<" + type + ">";
            }
        }
    }

    private Message GetMessage(OperationMessage operationMsg)
    {
        var messages = _services[0].Messages;
        return operationMsg != null ? messages[operationMsg.Message.Name] : null;
    }

    private void SetInputAndContentType(OperationWSDL ow, OperationBinding operationbinding)
    {
        if (operationbinding.Input is null)
        {
            ow.SendInputBodyAs = OperationBody.None;
            return;
        }

        ow.InputContentType = CoreConstants.XmlContentType;
        SetInput(ow, operationbinding.Input);
    }

    private void SetInput(OperationWSDL ow, InputBinding inputBinding)
    {
        if (inputBinding.Extensions.Find(typeof(SoapBodyBinding)) is SoapBodyBinding soap)
        {
            ow.BodeRequired = soap.Required;
            ow.BodyEncoding = soap.Encoding;
            ow.BodyNamespace = soap.Namespace;
            ow.BodyUse = soap.Use;
            ow.SendInputBodyAs = OperationBody.XmlBody;
        }
        else if (inputBinding.Extensions.Find(typeof(SoapHeaderBinding)) != null)
        {
            ow.SendInputBodyAs = OperationBody.HeaderParameter;
        }
        else if (inputBinding.Extensions.Find(typeof(MimeContentBinding)) is MimeContentBinding mimeContentBinding)
        {
            ow.InputContentType = mimeContentBinding.Type;
            ow.SendInputBodyAs = OperationBody.XmlBody;
        }
        else
        {
            if (ow.HttpVerb?.ToLower() == "get")
                ow.SendInputBodyAs = OperationBody.QueryParameter;
            else
                ow.SendInputBodyAs = OperationBody.UrlEncodedParametersBody;
        }
    }


    private void SetOutputAndContentType(OperationWSDL ow, OperationBinding operationbinding)
    {
        if (operationbinding.Output is null)
        {
            ow.ResponseOutputAs = OperationBody.None;
            return;
        }

        SetOutput(ow, operationbinding.Output);
    }

    private void SetOutput(OperationWSDL ow, OutputBinding outputBinding)
    {
        ow.OutputContentType = CoreConstants.XmlContentType;
        ow.ResponseOutputAs = OperationBody.XmlBody;
        if (outputBinding.Extensions.Find(typeof(SoapBodyBinding)) != null)
            ow.ResponseOutputAs = OperationBody.XmlBody;
        else
            SetMimeContentBinding(outputBinding, ow);
    }

    private void SetMimeContentBinding(OutputBinding outputBinding, OperationWSDL ow)
    {
        if (outputBinding.Extensions.Find(typeof(MimeContentBinding)) is not MimeContentBinding mimeContentBinding)
            return;
        ow.OutputContentType = mimeContentBinding.Type;
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

    private string GetHttpVerbFromBinding(Binding binding)
    {
        if (binding.Extensions.Find(typeof(HttpBinding)) is HttpBinding httpBinding)
            return httpBinding.Verb;
        return ApiConstants.POST;
    }


    private string GetActionUrl(string baseUrl, OperationBinding operationbinding)
    {
        var actionUrl = string.Empty;
        if (operationbinding.Extensions.Find(typeof(SoapOperationBinding)) is SoapOperationBinding soupOperation)
            actionUrl = soupOperation.SoapAction;

        if (operationbinding.Binding.Extensions.Find(typeof(HttpBinding)) != null)
        {
            var httpBinding = operationbinding.Extensions.Find(typeof(HttpOperationBinding)) as HttpOperationBinding;
            actionUrl = httpBinding.Location;
        }
        actionUrl = !actionUrl.StartsWith(CoreConstants.Slash) && !string.IsNullOrEmpty(actionUrl) ? actionUrl : $"{baseUrl}{actionUrl}";
        return actionUrl;
    }

    private string GetAddress(Port port)
    {
        if (port.Extensions.Find(typeof(SoapAddressBinding)) is SoapAddressBinding soapAddress)
            return soapAddress.Location;
        if (port.Extensions.Find(typeof(HttpAddressBinding)) is HttpAddressBinding httpAddress)
            return httpAddress.Location;
        return string.Empty;
    }


    internal List<WSDLServiceTypes> Parse(string path)
    {
        var serviceDescription = ServiceDescription.Read(path);
        return serviceDescription != null ? Parse(serviceDescription) : new() { };
    }

    internal List<WSDLServiceTypes> Parse(Stream stream)
    {
        var serviceDescription = ServiceDescription.Read(stream);
        return serviceDescription != null ? Parse(serviceDescription) : new() { };
    }
}

