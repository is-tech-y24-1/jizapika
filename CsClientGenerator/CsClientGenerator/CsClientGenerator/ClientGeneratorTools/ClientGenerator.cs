using System.Text;
using CsClientGenerator.Dictionary;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CsClientGenerator.ClientGeneratorTools;

public static class ClientGenerator
{
    public static void Generate(string serverClientPath, string generatingDirectory)
    {
        var serverClientCode = File.ReadAllText(serverClientPath, Encoding.UTF8);
        var clientInfo = ClientParser.Parse(serverClientCode);
        var generatingClientCode = GenerateClientCode(clientInfo).NormalizeWhitespace().ToString();
        var generatingClientDirectory = Path.Combine(generatingDirectory, "Clients");
        if (!Directory.Exists(generatingClientDirectory)) Directory.CreateDirectory(generatingClientDirectory);
        var generatingClientClass = Path.Combine(generatingClientDirectory, clientInfo.className + ".cs");
        if (File.Exists(generatingClientClass)) File.Delete(generatingClientClass);
        File.WriteAllText(generatingClientClass, generatingClientCode);
    }

    private static SyntaxNode GenerateClientCode(ClientInfo clientInfo) =>
        SyntaxFactory.CompilationUnit()
            .WithUsings(
                SyntaxFactory.List(
                    new []
                    {
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.IdentifierName("System"),
                                SyntaxFactory.IdentifierName("Text")
                            )
                        ),
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.IdentifierName("Newtonsoft"),
                                SyntaxFactory.IdentifierName("Json")
                            )
                        ),
                        SyntaxFactory.UsingDirective(
                                SyntaxFactory.QualifiedName(
                                    SyntaxFactory.QualifiedName(
                                        SyntaxFactory.QualifiedName(
                                            SyntaxFactory.IdentifierName("System"),
                                            SyntaxFactory.IdentifierName("Text")
                                        ),
                                        SyntaxFactory.IdentifierName("Json")
                                    ),
                                    SyntaxFactory.IdentifierName("JsonSerializer")
                                )
                            )
                            .WithAlias(
                                SyntaxFactory.NameEquals(
                                    SyntaxFactory.IdentifierName("JsonSerializer")
                                )
                            )
                    }
                )
            )
            .WithMembers(
                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                    SyntaxFactory.FileScopedNamespaceDeclaration(
                            SyntaxFactory.IdentifierName("CsClientGenerator")
                        )
                        .WithMembers(
                            SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                                GenerateClientClass(clientInfo)
                            )
                        )
                )
            );

    private static ClassDeclarationSyntax GenerateClientClass(ClientInfo clientInfo)
    {
        var list = GetHostFields();
        list.AddRange(clientInfo.methods.Select(GenerateMethod));

        return SyntaxFactory.ClassDeclaration(clientInfo.className)
            .WithMembers(SyntaxFactory.List(list));
    }

    private static MethodDeclarationSyntax GenerateMethod(Method method)
    {
        return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.IdentifierName(
                    MyTypeConverter.TaskWrapping(
                        MyTypeConverter.ConvertString(method.returnedType)
                    )
                ),
                SyntaxFactory.Identifier(method.methodName)
            )
            .WithModifiers(MethodModifiers())
            .WithParameterList(ArgumentList(method.arguments))
            .WithBody(SyntaxFactory.Block(GetMethodBlock(method)));
    }

    private static List<StatementSyntax> GetMethodBlock(Method method)
    {
        var list = new List<StatementSyntax> {SyntaxFactory.LocalDeclarationStatement(UrlAssignment(method.methodUrl))};
        list.AddRange(method.returnedType == "void" ? GetVoidMethodLogic(method) : GetReturningMethodLogic(method));

        return list;
    }

    private static List<StatementSyntax> GetVoidMethodLogic(Method method) =>
        HasRequestBodyArgument(method.arguments)
            ? GetCodeWithArgumentSerialization(method) 
            : GetCodeWithoutArgumentSerialization(method);

    private static List<StatementSyntax> GetReturningMethodLogic(Method method) =>
        GetCodeWithAnswerDeserialization(method);

    private static List<StatementSyntax> GetCodeWithArgumentSerialization(Method method)
    {
        return new List<StatementSyntax>
        {
            SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName("var")
                    )
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier("json")
                                )
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName("JsonConvert"),
                                                    SyntaxFactory.IdentifierName("SerializeObject")
                                                )
                                            )
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.IdentifierName(
                                                                RequestBodyArgumentName(method.arguments)
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                    )
                                )
                        )
                    )
            ),
            SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName("var")
                    )
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier("content")
                                )
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.ObjectCreationExpression(
                                                SyntaxFactory.IdentifierName("StringContent")
                                            )
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName("json")
                                                            ),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName("Encoding.UTF8")
                                                            ),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName("\"application/json\"")
                                                            )
                                                        }
                                                    )
                                                )
                                            )
                                    )
                                )
                        )
                    )
            ),
            SyntaxFactory.ExpressionStatement(
                HttpClientRequest(method.requestType)
            )
        };
    }

    private static string RequestBodyArgumentName(List<Argument> arguments)
    {
        if (!HasRequestBodyArgument(arguments))
            throw new Exception("The arguments don't have RequestBody parameter");
        return arguments.First(a => a.annotation == "@RequestBody").argumentName;
    }

    private static List<StatementSyntax> GetCodeWithoutArgumentSerialization(Method method)
    {
        return new List<StatementSyntax>
        {
            SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName("var")
                    )
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier("content")
                                )
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.ObjectCreationExpression(
                                                SyntaxFactory.IdentifierName("StringContent")
                                            )
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                        {
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName(
                                                                    GetStringContentWithRequestParams(method.arguments)
                                                                )
                                                            ),
                                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName("Encoding"),
                                                                    SyntaxFactory.IdentifierName("UTF8")
                                                                )
                                                            )
                                                        }
                                                    )
                                                )
                                            )
                                    )
                                )
                        )
                    )
            ),
            SyntaxFactory.ExpressionStatement(
                HttpClientRequest(method.requestType)
            )
        };
    }

    private static AwaitExpressionSyntax HttpClientRequest(string requestType) =>

        SyntaxFactory.AwaitExpression(
            SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("HttpClient"),
                        SyntaxFactory.IdentifierName(requestType + "Async")
                    )
                )
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.IdentifierName(
                                    requestType is "Put" or "Post" or "Patch"
                                        ? "Host + url, content"
                                        : "Host + url"
                                )
                            )
                        )
                    )
                )
        );

    private static string GetStringContentWithRequestParams(List<Argument> arguments) =>
        arguments.Exists(a => a.annotation == "@RequestParam")
            ? "$\"?" + string.Join("&", arguments.Select(
                    a => a.annotation == "@RequestParam"
                        ? ClearingArgumentName(a.argumentName)
                          + "={" + ClearingArgumentName(a.argumentName) + "}"
                        : ""
                )
            ) + "\""
            : "\"\"";

    private static string ClearingArgumentName(string argumentName) =>
        argumentName.Replace("\"", "").Replace(" ", "");

    private static List<StatementSyntax> GetCodeWithAnswerDeserialization(Method method)
    {
        return new List<StatementSyntax>
        {
            SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName(
                            "var"
                        )
                    )
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier("response")
                                )
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        HttpClientRequest(method.requestType)
                                    )
                                )
                        )
                    )
            ),
            SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName(
                            "var"
                        )
                    )
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier("content")
                                )
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.AwaitExpression(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.IdentifierName("response"),
                                                        SyntaxFactory.IdentifierName("Content")
                                                    ),
                                                    SyntaxFactory.IdentifierName("ReadAsStringAsync")
                                                )
                                            )
                                        )
                                    )
                                )
                        )
                    )
            ),
            SyntaxFactory.ReturnStatement(
                SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("JsonSerializer"),
                            SyntaxFactory.GenericName(
                                    SyntaxFactory.Identifier("Deserialize")
                                )
                                .WithTypeArgumentList(
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                            SyntaxFactory.IdentifierName(method.returnedType)
                                        )
                                    )
                                )
                        )
                    )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.IdentifierName("content")
                                )
                            )
                        )
                    )
            )
        };
    }

    private static bool HasRequestBodyArgument(List<Argument> arguments) =>
        arguments.Any(argument => argument.annotation == "@RequestBody");

    private static ExpressionStatementSyntax RequestImplementation(string requestType) =>
        SyntaxFactory.ExpressionStatement(
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("HttpClient"),
                            SyntaxFactory.IdentifierName(requestType + "Async")
                        )
                    )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                RequestImplementationArguments(requestType)
                            )
                        )
                    )
            )
        );

    private static SyntaxNodeOrToken[] RequestImplementationArguments(string requestType)
    {
        var arguments = new List<SyntaxNodeOrToken>
        {
            SyntaxFactory.Argument(
                SyntaxFactory.BinaryExpression(
                    SyntaxKind.AddExpression,
                    SyntaxFactory.IdentifierName("Host"),
                    SyntaxFactory.IdentifierName("url")
                )
            )
        };

        if (requestType is "Put" or "Post")
        {
            arguments.Add(
                SyntaxFactory.Token(SyntaxKind.CommaToken)
            );
            arguments.Add(
                SyntaxFactory.Argument(
                    SyntaxFactory.IdentifierName("data")
                )
            );
        }

        return arguments.ToArray();
    }
    private static VariableDeclarationSyntax UrlAssignment(string url) =>
        SyntaxFactory.VariableDeclaration(
                SyntaxFactory.IdentifierName(
                    SyntaxFactory.Identifier("var")
                )
            )
            .WithVariables(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier("url")
                        )
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.IdentifierName("$" + url)
                            )
                        )
                )
            );

    private static ParameterListSyntax ArgumentList(List<Argument> arguments)
    {
        var list = new List<SyntaxNodeOrToken>();
        for (var i = 0; i < arguments.Count - 1; i++)
        {
            list.Add(GetArgumentNode(arguments[i]));
            list.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
        }
        if(arguments.Count > 0)
            list.Add(GetArgumentNode(arguments[^1]));

        return
            SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList<ParameterSyntax>(list)
            );
    }

    private static ParameterSyntax GetArgumentNode(Argument argument) =>
        SyntaxFactory.Parameter(
                SyntaxFactory.Identifier(argument.argumentName)
            )
            .WithType(
                SyntaxFactory.IdentifierName(argument.argumentType)
            );

    private static List<MemberDeclarationSyntax> GetHostFields()
    {
        var newHttpClient = SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.IdentifierName("HttpClient")
            )
            .WithArgumentList(
                SyntaxFactory.ArgumentList()
            );
        var hostValue = SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal("http://localhost:8080")
        );

        return new List<MemberDeclarationSyntax>
        {
            GenerateField("HttpClient", "HttpClient", newHttpClient),
            GenerateField("string", "Host", hostValue)
        };
    }

    private static FieldDeclarationSyntax GenerateField(string fieldType, string fieldName, ExpressionSyntax value) =>
        SyntaxFactory.FieldDeclaration(
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.IdentifierName(fieldType)
            )
            .WithVariables(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(
                        SyntaxFactory.Identifier(fieldName)
                    )
                    .WithInitializer(
                        SyntaxFactory.EqualsValueClause(value)
                    )
                )
            )
        )
        .WithModifiers(FieldModifiers());

    private static SyntaxTokenList FieldModifiers() =>
        SyntaxFactory.TokenList(
            SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
            SyntaxFactory.Token(SyntaxKind.StaticKeyword),
            SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
        );

    private static SyntaxTokenList MethodModifiers() =>
        SyntaxFactory.TokenList(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.StaticKeyword),
            SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
        );
}