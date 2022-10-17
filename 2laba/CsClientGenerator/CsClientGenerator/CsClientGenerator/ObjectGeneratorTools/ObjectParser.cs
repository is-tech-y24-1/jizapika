namespace CsClientGenerator.ObjectGeneratorTools;

public static class ObjectParser
{
    public static ObjectInfo Parse(string javaObjectCode)
    {
        var objectInfo = new ObjectInfo(ClassName(javaObjectCode));
        var lines = GetClassCode(javaObjectCode).Split('\n', '\r');
        foreach (var line in lines)
        {
            if (!line.Contains(";") || line.Contains("{")) continue;
            var lineWithoutSemicolon = line.Replace(";", "");
            var words = lineWithoutSemicolon.Split(" ");
            int accessModifierId;
            if (Array.Exists(words, s => s.Equals("protected")))
            {
                accessModifierId = Array.IndexOf(words, "protected");
            }
            else if (Array.Exists(words, s => s.Equals("private")))
            {
                accessModifierId = Array.IndexOf(words, "private");
            }
            else if (Array.Exists(words, s => s.Equals("public")))
            {
                accessModifierId = Array.IndexOf(words, "public");
            }
            else continue;
            objectInfo.AddField(new Field(words[accessModifierId + 1], words[accessModifierId + 2]));
        }
        return objectInfo;
    }

    private static string ClassName(string javaObjectCode)
    {
        var words = javaObjectCode.Split(' ', '\n', '\r');
        var classIndexInArray = Array.FindIndex(words, s => s.Equals("class"));
        var nextWord = words.ElementAt(classIndexInArray + 1);
        if (nextWord.Contains('{')) nextWord.Remove(nextWord.IndexOf('{', StringComparison.Ordinal));
        return nextWord;
    }

    private static string GetClassCode(string javaObjectCode)
    {
        var beginClassInfoIndex = javaObjectCode.IndexOf("{", StringComparison.Ordinal);
        var endClassInfoIndex = javaObjectCode.LastIndexOf("}", StringComparison.Ordinal);
        return javaObjectCode.Substring(
            beginClassInfoIndex + 1, endClassInfoIndex - beginClassInfoIndex - 1);
    }
}