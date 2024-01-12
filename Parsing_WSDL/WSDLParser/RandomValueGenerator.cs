using System;

class SampleGenerator
{
    private static Random random = new Random();

    public static string GetSampleValue(string typeName)
    {
        switch (typeName.ToLower())
        {
            case "bool":
                return (random.Next(2) == 0).ToString(); // Random boolean as string
            case "string":
                return "<string>";
            case "decimal":
                return (random.NextDouble() * 100).ToString(); // Random decimal between 0 and 100 as string
            case "int":
                return random.Next().ToString(); // Random integer as string
            case "double":
                return random.NextDouble().ToString(); // Random double between 0 and 1 as string
            default:
                return "<string>";
        }
    }

}
