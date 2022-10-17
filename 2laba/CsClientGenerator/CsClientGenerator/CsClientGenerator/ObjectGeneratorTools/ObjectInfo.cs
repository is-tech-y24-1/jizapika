using Microsoft.VisualBasic;

namespace CsClientGenerator.ObjectGeneratorTools;

public class ObjectInfo
{
    public string className { get; }
    public List<Field> fields { get; }
    public ObjectInfo(string className)
    {
        this.className = className;
        fields = new List<Field>();
    }

    public void AddField(Field field) => fields.Add(field);
}