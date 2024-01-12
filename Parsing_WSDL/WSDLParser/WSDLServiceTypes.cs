using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Services.Description;

[Flags]
public enum RestParameterLocationType
{
    None = 0,
    Url = 0x1,
    Query = 0x2,
    Body = 0x4,
    Cookie = 0x8,
    Header = 0x10
}


public class Parameter
{
    public Parameter(string name, string type, string defaultValue, OperationBody operationInputBodyType)
    {
        this.Name = name;
        this.Type = type;
        DefaultValue = defaultValue;
        LocationType = GetRestParameterLocationType(operationInputBodyType);
    }

    private RestParameterLocationType GetRestParameterLocationType(OperationBody operationInputBodyType)
    {
        switch (operationInputBodyType)
        {
            case OperationBody.QueryParameter:
                return RestParameterLocationType.Query;
            case OperationBody.HeaderParameter:
                return RestParameterLocationType.Header;
            case OperationBody.UrlEncodedParametersBody:
            case OperationBody.XmlBody:
                return RestParameterLocationType.Body;
            default: return RestParameterLocationType.None;
        }
    }

    public string Name { get; set; }
    public string Type { get; set; }
    public string DefaultValue { get; set; }
    public RestParameterLocationType LocationType { get; set; }
}


public enum OperationBody
{
    None,
    UrlEncodedParametersBody,
    XmlBody,
    QueryParameter,
    HeaderParameter

}


public class ParameterCollection : List<Parameter>
{

}



public class OperationWSDL
{
    public string name { get; set; }
    public string actionUrl { get; set; }
    public string input { get; set; }
    public string output { get; set; }
    public ParameterCollection InputParameters { get; set; } = new();
    public ParameterCollection OutputParameters { get; set; } = new();

    public OperationBody SendInputBodyAs { get; set; }
    public OperationBody ResponseOutputAs { get; set; }

    public string InputContentType { get; set; }
    public string OutputContentType { get; set; }
    public string Description { get; set; }
    public string BodyNamespace { get; set; }
    public SoapBindingUse BodyUse { get; set; }
    public bool BodeRequired { get; set; }
    public string BodyEncoding { get; set; }
    public string HttpVerb { get; set; }
}

public class PortWSDL
{
    public string name { get; set; }
    public string address { get; set; }
    public List<OperationWSDL> operations { get; set; }
}

public class WSDLServiceTypes
{
    public string name { get; set; }
    public List<PortWSDL> port { get; set; }
}

