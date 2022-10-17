namespace CsClientGenerator.ClientGeneratorTools;

public class ClientInfo
{
    
    public string className { get; }
    public string classUrl { get; }
    public List<Method> methods { get; }

    public ClientInfo(string className, string classUrl)
    {
        this.className = className;
        this.classUrl = classUrl;
        methods = new List<Method>();
    }

    public void AddMethod(Method method) => methods.Add(method);
}