using System;
using System.Net;
using System.Xml;
using Newtonsoft.Json;
using System.Web.Services.Description;

internal class Program
{
    private static void Main(string[] args)
    {
        //string pathToSave = @"C:\Users\aftab.ahmed\Downloads\Unit Test\";
        //string prev = "C:\\Users\\aftab.ahmed\\Downloads\\wsdl_temp_conversion.wsdl"; 
        //string prev = @"C:\Users\aftab.ahmed\Downloads\soap_doclit.wsdl";
        //string prev = @"C:\Users\aftab.ahmed\Downloads\global_weather_wsdl.wsdl";
        //string prev = @"C:\Users\aftab.ahmed\Downloads\wsdl_sample.wsdl";
        //string prev = @"C:\Users\aftab.ahmed\source\repos\Parsing_WSDL\Parsing_WSDL\Example_Wsdl_to_parse\corporatesms.svc.wsdl";
        //string prev = @"C:\Users\aftab.ahmed\source\repos\Parsing_WSDL\Parsing_WSDL\Example_Wsdl_to_parse\salesforce.enterprise.wsdl";
        string prev = @"C:\Users\aftab.ahmed\source\repos\Parsing_WSDL\Parsing_WSDL\Example_Wsdl_to_parse\ndfdXMLserver.php.wsdl";

        var services = new WSDLParser().Parse(prev);

        //var json = JsonConvert.SerializeObject(services);
        //File.WriteAllText(pathToSave, json);

    }


}