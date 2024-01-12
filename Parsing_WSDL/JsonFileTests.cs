using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

[TestClass]
public static class JsonFileTests
{
    [TestMethod]
    public static void TestJsonFileContentEquality()
    {
        // Specify the paths to your JSON files
        string filePath1 = @"C:\Users\aftab.ahmed\Downloads\Unit Test\1.json";
        string filePath2 = @"C:\Users\aftab.ahmed\Downloads\Unit Test\2.json";

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
