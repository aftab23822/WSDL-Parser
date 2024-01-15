using System.Data;
using System.Web.Services.Description;
using Parsing_WSDL.WSDLParser;

public class WSDLParser
{
    private ServiceDescriptionCollection _services = new();
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
        HandleIntputOutput(operationWsdl, operationbinding, operation);
        portWsdl.operations.Add(operationWsdl);
    }

    private void HandleIntputOutput(OperationWSDL operationWsdl, OperationBinding operationbinding, Operation? operation)
    {
        var contentTypeHandler = new ContentTypeHandler();
        contentTypeHandler.SetInputOutputContentType(operationWsdl, operationbinding);

        var bodyHandler = new MessagePartHandler(_services);
        bodyHandler.HanldeInputOutputBody(operation, operationWsdl);
    }


    private string GetHttpVerbFromBinding(Binding binding)
    {
        if (binding.Extensions.Find(typeof(HttpBinding)) is HttpBinding httpBinding)
            return httpBinding.Verb;
        return ApiConstants.POST;
    }


    private string GetActionUrl(string baseUrl, OperationBinding operationbinding)
    {
        var actionUrl = GetActionFromOperBinding(operationbinding);
        actionUrl = !actionUrl.StartsWith(ApiConstants.Slash) && !string.IsNullOrEmpty(actionUrl) ? actionUrl : $"{baseUrl}{actionUrl}";
        return actionUrl;
    }

    private string GetActionFromOperBinding(OperationBinding operationbinding)
    {
        if (operationbinding.Extensions.Find(typeof(SoapOperationBinding)) is SoapOperationBinding soupOperation)
            return soupOperation.SoapAction;

        if (operationbinding.Binding.Extensions.Find(typeof(HttpBinding)) != null)
        {
            var httpBinding = operationbinding.Extensions.Find(typeof(HttpOperationBinding)) as HttpOperationBinding;
            return httpBinding.Location;
        }
        return string.Empty;
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

