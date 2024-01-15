using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

[TestClass]
public static class JsonFileTests
{
    [TestMethod]
    public static void TestJsonFileContentEquality()
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

        foreach (string filePath in filePaths)
        {
            // Specify the paths to your JSON files
            string filePath1 = filePath.Replace("Example_Wsdl_to_parse", "TestCases") + ".json";
            string filePath2 = filePath.Replace("Example_Wsdl_to_parse", "ComparisonJsons") + ".json";

            // Read the content of the JSON files
            string jsonContent1 = File.ReadAllText(filePath1);
            string jsonContent2 = File.ReadAllText(filePath2);

            // Parse the JSON content into JObjects for comparison
            JArray json1 = JArray.Parse(jsonContent1);
            JArray json2 = JArray.Parse(jsonContent2);

            // Assert that the two JSON objects are equal
            Assert.IsTrue(JToken.DeepEquals(json1, json2), "The content of the JSON files is not equal.");
        }
    }
}
