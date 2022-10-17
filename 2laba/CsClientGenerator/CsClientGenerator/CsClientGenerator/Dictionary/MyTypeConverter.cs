using String = System.String;

namespace CsClientGenerator.Dictionary;

public static class MyTypeConverter
{
    public static string ConvertString(string toConvert)
    {
        if (!toConvert.Contains('<')) return ConvertType(toConvert);
        var subTypeBegin = toConvert.IndexOf("<", StringComparison.Ordinal);
        var subTypeEnd = toConvert.LastIndexOf(">", StringComparison.Ordinal);
        return ConvertType(toConvert.Substring(0, subTypeBegin))
               + "<" + ConvertString(toConvert.Substring(subTypeBegin + 1, subTypeEnd - subTypeBegin - 1)) + ">";
    }

    public static string TaskWrapping(string toConvert)
    {
        var convertType = ConvertType(toConvert);
        return convertType switch
        {
            "void" => "Task",
            _ => "Task<" + convertType + "?>"
        };
    }

    private static string ConvertType(string toConvert)
    {
        return toConvert switch
        {
            "String" => "string",
            "Integer" => "int",
            "Boolean" => "bool",
            "ArrayList" => "List",
            "Date" => "DateTime",
            _ => toConvert
        };
    }
}