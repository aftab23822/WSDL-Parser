using System;

class SampleGenerator
{
    private static Random random = new Random();

    public static string XGetSampleValue(string typeName)
    {
        switch (typeName.ToLower())
        {
            case "bool":
            case "boolean":
                return (random.Next(2) == 0).ToString(); 
            case "string":
                return "string";
            case "decimal":
                return Math.Round(random.NextDouble() * 100, 2).ToString(); 
            case "int":
                return random.Next().ToString(); 
            case "double":
                return Math.Round(random.NextDouble(), 2).ToString();
            default:
                return "string";
        }
    }

    public static string GetSampleValue(string typeName)
    {
        switch (typeName.ToLower())
        {
            default:
                return "string";
        }
    }

}
