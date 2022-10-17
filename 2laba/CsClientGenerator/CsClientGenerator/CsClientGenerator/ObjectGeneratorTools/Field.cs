namespace CsClientGenerator.ObjectGeneratorTools;

public class Field
{
    public string fieldType { get; }
    public string fieldName { get; }

    public Field(string fieldType, string fieldName)
    {
        this.fieldType = fieldType;
        this.fieldName = fieldName;
    }
}