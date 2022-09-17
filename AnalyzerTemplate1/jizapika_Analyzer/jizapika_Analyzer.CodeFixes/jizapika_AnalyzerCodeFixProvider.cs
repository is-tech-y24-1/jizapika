using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace jizapika_Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(jizapika_AnalyzerCodeFixProvider)), Shared]
    public class jizapika_AnalyzerCodeFixProvider : CodeFixProvider
    {
        public const string title = "Make correct Try-method.";
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(jizapika_AnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync();
            var diagnostics = context.Diagnostics;
            if (diagnostics.Count() != 1) throw new Exception("Diagnostics are not correct!");

            var diagnostic = diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var methodNode = root.FindNode(diagnosticSpan);

            if (!methodNode.IsKind(SyntaxKind.MethodDeclaration)) throw new Exception("There isn't method.");

            var predefinedOrIdentifierNodes = methodNode.ChildNodes().Where(
                    node => node.IsKind(SyntaxKind.PredefinedType) || node.IsKind(SyntaxKind.IdentifierName));

            if (predefinedOrIdentifierNodes.Count() == 0) throw new Exception("Method has not returned type.");

            var predefinedNode = predefinedOrIdentifierNodes.First();

            var predefinedTypes = predefinedNode.ChildTokens();

            if (predefinedTypes.Count() != 1) throw new Exception("Predefined node must have 1 type on childs.");

            // predefinedType это токен, содержащий либо keyword, либо string. Например, (Токен "Student") или (Токен "int").
            var predefinedType = predefinedTypes.First();

            var parameterListNodes = methodNode.ChildNodes().Where(node => node.IsKind(SyntaxKind.ParameterList));

            if (parameterListNodes.Count() != 1) throw new Exception("Method must have 1 ParameterList node.");

            var parameterListNode = parameterListNodes.First();

            context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CodeFixTitle,
                createChangedDocument: async c =>
                {
                    var editor = new SyntaxEditor(root, document.Project.Solution.Workspace);

                    editor.ReplaceNode(predefinedNode, BoolNode());

                    var newParameterListNode = NewParameterList(parameterListNode, predefinedType.ToString());

                    editor.ReplaceNode(parameterListNode, newParameterListNode);
                    var newParameterName =
                        newParameterListNode.ChildNodes().First(node => node.IsKind(SyntaxKind.Parameter)).ChildTokens().Where(name => name.ToString() != "out").First().ToString();

                    ChangeMethodBlock(methodNode, ref editor, newParameterName);

                    var newRoot = editor.GetChangedRoot().NormalizeWhitespace();

                    var newDocument = document.WithSyntaxRoot(newRoot);
                    return newDocument;
                },
                equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
            diagnostic);
        }

        private static void ChangeMethodBlock(SyntaxNode method, ref SyntaxEditor editor, string outVariableName)
        {
            var blocks = method.ChildNodes().Where(node => node.IsKind(SyntaxKind.Block));

            if (blocks.Count() == 1)
            {
                var block = blocks.First();

                var returnStatements = block.DescendantNodes().Where(node => node.IsKind(SyntaxKind.ReturnStatement));

                var returnNewObjectStatements = returnStatements.Where(
                        node => node.ChildNodes().Where(
                            childNode => childNode.IsKind(SyntaxKind.ObjectCreationExpression)).Count() == 1);

                var returnNewObjectStatementsInIf =
                    returnNewObjectStatements.Where(
                        node => node.Ancestors().Where(
                            ancestor => ancestor.IsKind(SyntaxKind.IfStatement) && method.Contains(ancestor)
                        )
                        .Count() != 0
                    );

                var returnNewObjectStatementsInIfWithoutBlock =
                    returnNewObjectStatementsInIf.Where(
                        node => !node.Parent.IsKind(SyntaxKind.Block));

                foreach (var returnStatement in returnStatements)
                {
                    var returnObjectStatement = returnStatement.ChildNodes().First();
                    if (returnNewObjectStatementsInIfWithoutBlock.Contains(returnStatement))
                    {
                        editor.ReplaceNode(returnStatement, BlockCreatingWithReturnStatement(outVariableName, returnObjectStatement));
                    }

                    else
                    {
                        editor.InsertAfter(returnStatement, ReturnTrueNode());
                        editor.ReplaceNode(returnStatement, OutVariableDeclaration(outVariableName, returnObjectStatement));
                    }
                }
            }
        }

        private static BlockSyntax BlockCreatingWithReturnStatement(string outVariableName, SyntaxNode returnObjectStatement)
        {
            return
                SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(outVariableName),
                            SyntaxFactory.IdentifierName(returnObjectStatement.ToString())
                        )
                    ),
                    ReturnTrueNode()
                )
                .NormalizeWhitespace();
        }

        private static ExpressionStatementSyntax OutVariableDeclaration (string outVariableName, SyntaxNode returnObjectStatement)
        {
            return
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName(outVariableName),
                        SyntaxFactory.IdentifierName(returnObjectStatement.ToString())
                    )
                )
                .NormalizeWhitespace();
        }

        private static ReturnStatementSyntax ReturnTrueNode()
        {
            return
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.TrueLiteralExpression
                    )
                )
                .NormalizeWhitespace();
        }

        private static ParameterListSyntax NewParameterList(SyntaxNode oldParameterList, string newOutParameterType)
        {

            var parameters = oldParameterList.ChildNodes().Where(node => node.IsKind(SyntaxKind.Parameter));

            var iteratingOutParameterName = "value";

            var newOutParameterName = iteratingOutParameterName;

            int idForName = 1;

            while (
                parameters.Where(
                    parameter => parameter.ChildTokens().Last().ToString() == newOutParameterName)
                .Count() != 0)
            {
                newOutParameterName = iteratingOutParameterName + idForName.ToString();
                idForName++;
            }

            var newOutParameter =
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier(newOutParameterName)
                )
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.OutKeyword)
                    )
                )
                .WithType(
                    SyntaxFactory.IdentifierName(newOutParameterType)
                );

            var newParameters = new List<SyntaxNodeOrToken>
            {
                newOutParameter
            };

            foreach (var parameter in parameters)
            {
                newParameters.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));

                newParameters.Add(parameter);
            }

            return
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SeparatedList<ParameterSyntax>(
                        newParameters
                    )
                );
        }

        private static PredefinedTypeSyntax BoolNode()
        {
            return
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.BoolKeyword)
                );
        }
    }
}