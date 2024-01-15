﻿using Newtonsoft.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        string[] filePaths =
        {
            @"C:\Users\aftab.ahmed\source\repos\Parsing_WSDL\Parsing_WSDL\Example_Wsdl_to_parse\wsdl_temp_conversion.wsdl",
            @"C:\Users\aftab.ahmed\source\repos\Parsing_WSDL\Parsing_WSDL\Example_Wsdl_to_parse\soap_doclit.wsdl",
            @"C:\Users\aftab.ahmed\source\repos\Parsing_WSDL\Parsing_WSDL\Example_Wsdl_to_parse\ndfdXMLserver.php.wsdl",
            @"C:\Users\aftab.ahmed\source\repos\Parsing_WSDL\Parsing_WSDL\Example_Wsdl_to_parse\global_weather_wsdl.wsdl",
            @"C:\Users\aftab.ahmed\source\repos\Parsing_WSDL\Parsing_WSDL\Example_Wsdl_to_parse\wsdl_sample.wsdl",
            @"C:\Users\aftab.ahmed\source\repos\Parsing_WSDL\Parsing_WSDL\Example_Wsdl_to_parse\corporatesms.svc.wsdl",
            @"C:\Users\aftab.ahmed\source\repos\Parsing_WSDL\Parsing_WSDL\Example_Wsdl_to_parse\salesforce.enterprise.wsdl"
        };

        var services = new WSDLParser().Parse(filePaths[0]);


        //TESTING BEFORE AND AFTER CODE REFACTORING CASE

        //foreach (string file in filePaths)
        //{
        //    //string pathToSave = file.Replace("Example_Wsdl_to_parse", "TestCases") + ".json";
        //    string pathToSave2 = file.Replace("Example_Wsdl_to_parse", "ComparisonJsons") + ".json";
        //    var services = new WSDLParser().Parse(file);
        //    var json = JsonConvert.SerializeObject(services);
        //    //File.WriteAllText(pathToSave, json);
        //    File.WriteAllText(pathToSave2, json);
        //}

        //JsonFileTests.TestJsonFileContentEquality();
    }
}