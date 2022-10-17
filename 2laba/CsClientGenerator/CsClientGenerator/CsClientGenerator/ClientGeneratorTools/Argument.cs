namespace CsClientGenerator.ClientGeneratorTools;

public class Argument
{
 
    public string annotation { get; }
    public string argumentType { get; }
    public string argumentName { get; }

    public Argument(string annotation, string argumentType, string argumentName)
    {
        this.annotation = annotation;
        this.argumentType = argumentType;
        this.argumentName = argumentName;
    }
}