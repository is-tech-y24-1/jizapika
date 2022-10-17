using System.Reflection.Metadata;

namespace CsClientGenerator.ClientGeneratorTools;

public class Method
{
    public string requestType { get; }
    public string returnedType { get; }
    public string methodName { get; }
    public string methodUrl { get; }

    public List<Argument> arguments { get; }

    public Method(string requestType, string returnedType, string methodName, string methodUrl)
    {
        this.requestType = requestType;
        this.returnedType = returnedType;
        this.methodName = methodName;
        this.methodUrl = methodUrl;
        arguments = new List<Argument>();
    }

    public void AddArgument(Argument argument) => arguments.Add(argument);
}