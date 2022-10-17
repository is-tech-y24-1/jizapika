using System.Collections;
using System.Reflection;

namespace CsClientGenerator.ClientGeneratorTools;

public static class ClientParser
{
    public static ClientInfo Parse(string javaClientCode)
    {
        var className = ControllerName(javaClientCode)
            .Replace("Controller", "Client")
            .Replace("controller", "client");
        var classUrl = ClassUrl(javaClientCode);
        var clientInfo = new ClientInfo(className, classUrl);
        var remainingCode = GetClassCode(javaClientCode)
            .Replace("\r", "")
            .Replace("\n", "");
        while (Array.Exists(remainingCode.Split(' '),
                   w => w.Contains("@") && w.Contains("Mapping")))
        {
            clientInfo.AddMethod(GetMethodInfo(remainingCode, out var numberOfPassedCharacters));
            remainingCode = remainingCode.Remove(0, numberOfPassedCharacters);
        }
        return clientInfo;
    }

    private static string ControllerName(string javaClientCode)
    {
        var words = javaClientCode.Split(' ', '\n', '\r');
        var classIndexInArray = Array.FindIndex(words, s => s.Equals("class"));
        var nextWord = words.ElementAt(classIndexInArray + 1);
        if (nextWord.Contains('{')) nextWord.Remove(nextWord.IndexOf('{', StringComparison.Ordinal));
        return nextWord;
    }

    private static string ClassUrl(string javaClientCode)
    {
        var lines = javaClientCode.Split(' ');
        var classIndexInArray = Array.FindIndex(lines, s => s.Contains("@RequestMapping"));
        if (classIndexInArray == -1) return "";
        var requestMappingAnnotation = lines.ElementAt(classIndexInArray);
        var urlBegin = requestMappingAnnotation.IndexOf("\"", StringComparison.Ordinal);
        var urlEnd = requestMappingAnnotation.LastIndexOf("\"", StringComparison.Ordinal);
        var url = requestMappingAnnotation.Substring(urlBegin + 1, urlEnd - urlBegin - 1);
        return url;
    }

    private static string GetClassCode(string javaClientCode)
    {
        var beginClassInfoIndex = javaClientCode.IndexOf("{", StringComparison.Ordinal);
        var endClassInfoIndex = javaClientCode.LastIndexOf("}", StringComparison.Ordinal);
        return javaClientCode.Substring(
            beginClassInfoIndex + 1, endClassInfoIndex - beginClassInfoIndex - 1);
        
    }

    private static Method GetMethodInfo(string remainingCode, out int numberOfPassedCharacters)
    {
        var words = remainingCode.Split(' ');
        var mappingAnnotationLineNumber = Array.FindIndex(
            words, w => w.Contains("@") && w.Contains("Mapping"));
        var annotationFirstLetterNumber =
            remainingCode.IndexOfAny(words[mappingAnnotationLineNumber].ToCharArray());
        var remainingCodeCharArray = remainingCode.ToCharArray();
        var requestTypeBegin = -1;
        var requestTypeEnd = -1;
        for (var i = annotationFirstLetterNumber; i < remainingCodeCharArray.Length; i++)
        {
                
            if (remainingCodeCharArray[i] == '@') requestTypeBegin = i + 1;
            if (remainingCode.Substring(i, 7) != "Mapping" || requestTypeBegin == -1) continue;
            requestTypeEnd = i - 1;
            break;
        }

        if (requestTypeBegin == -1 || requestTypeEnd == -1)
            throw new Exception("bag: can't find annotation");
        var requestType =
            remainingCode.Substring(requestTypeBegin, requestTypeEnd - requestTypeBegin + 1);

        var methodUrlBegin = -1;
        var methodUrlEnd = -1;
        for (var i = requestTypeEnd; i < remainingCodeCharArray.Length; i++)
        {
            if (remainingCodeCharArray[i] == ')')
            {
                methodUrlEnd = i - 1;
                break;
            }

            if (remainingCodeCharArray[i] == '(') methodUrlBegin = i + 1;
        }

        if (methodUrlBegin == -1 || methodUrlEnd == -1)
            throw new Exception("incorrect Annotation");
        var url = remainingCode.Substring(methodUrlBegin, methodUrlEnd - methodUrlBegin + 1);


        var roundArgumentsBracketBegin =
            remainingCode.IndexOf("(", methodUrlEnd + 2, StringComparison.Ordinal);
        var signatureWithoutArguments = remainingCode.Substring(
            methodUrlEnd + 2, roundArgumentsBracketBegin - methodUrlEnd - 2);
        var signatureWords = signatureWithoutArguments.Split(" ");
        var returnedType = signatureWords[^2];
        var methodName = signatureWords[^1];
        
        var method = new Method(requestType, returnedType, methodName, url);

        var roundArgumentsBracketEnd =
            remainingCode.IndexOf(")", roundArgumentsBracketBegin, StringComparison.Ordinal);
        var argumentsCode = remainingCode.Substring(
            roundArgumentsBracketBegin + 1, roundArgumentsBracketEnd - roundArgumentsBracketBegin - 1);
        var arguments = argumentsCode.Split(",");
        foreach (var argument in arguments)
        {
            if (argument.Length > 0)
                method.AddArgument(GetArgumentInfo(argument));
        }

        numberOfPassedCharacters = roundArgumentsBracketEnd + 1;
        return method;
    }

    private static Argument GetArgumentInfo(string argumentCode)
    {
        var argumentWords = argumentCode.Split(" ");
        argumentWords = argumentWords.Where(w => !w.Equals("")).ToArray();
        var annotation = argumentWords[^3];
        var argumentType = argumentWords[^2];
        var argumentName = argumentWords[^1];
        return new Argument(annotation, argumentType, argumentName);
    }
}