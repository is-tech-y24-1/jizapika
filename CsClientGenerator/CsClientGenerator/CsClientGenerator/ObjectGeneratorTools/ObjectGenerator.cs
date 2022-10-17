using System.Text;
using CsClientGenerator.Dictionary;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CsClientGenerator.ObjectGeneratorTools;

public static class ObjectGenerator
{
    public static void Generate(string serverObjectPath, string generatingDirectory)
    {
        var serverObjectCode = File.ReadAllText(serverObjectPath, Encoding.UTF8);
        var objectInfo = ObjectParser.Parse(serverObjectCode);
        var generatingObjectCode = GenerateObjectCode(objectInfo).NormalizeWhitespace().ToString();
        var generatingObjectDirectory = Path.Combine(generatingDirectory, "Objects");
        if (!Directory.Exists(generatingObjectDirectory)) Directory.CreateDirectory(generatingObjectDirectory);
        var generatingObjectClass = Path.Combine(generatingObjectDirectory, objectInfo.className + ".cs");
        if (File.Exists(generatingObjectClass)) File.Delete(generatingObjectClass);
        File.WriteAllText(generatingObjectClass, generatingObjectCode);
    }

    private static SyntaxNode GenerateObjectCode(ObjectInfo objectInfo) =>
        SyntaxFactory.CompilationUnit()
            .WithMembers(
                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                    SyntaxFactory.FileScopedNamespaceDeclaration(
                            SyntaxFactory.IdentifierName("CsClientGenerator")
                        )
                        .WithMembers(
                            SyntaxFactory.SingletonList(
                                GenerateObjectClass(objectInfo)
                            )
                        )
                )
            );

    private static MemberDeclarationSyntax GenerateObjectClass(ObjectInfo objectInfo)
    {
        var list = new List<MemberDeclarationSyntax>();
        foreach (var field in objectInfo.fields)
        {
            list.Add(GenerateField(field));
        }

        return SyntaxFactory.ClassDeclaration(objectInfo.className)
            .WithMembers(
            SyntaxFactory.List(list));
    }

    private static MemberDeclarationSyntax GenerateField(Field field)
    {
        return SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.IdentifierName(MyTypeConverter.ConvertString(field.fieldType)),
                SyntaxFactory.Identifier(field.fieldName)
            )
            .WithModifiers(
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword)
                )
            )
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    SyntaxFactory.List<AccessorDeclarationSyntax>(
                        new AccessorDeclarationSyntax[]{
                            SyntaxFactory.AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration
                                )
                                .WithSemicolonToken(
                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                ),
                            SyntaxFactory.AccessorDeclaration(
                                    SyntaxKind.SetAccessorDeclaration
                                )
                                .WithSemicolonToken(
                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                )
                        }
                    )
                )
            );
    }
}