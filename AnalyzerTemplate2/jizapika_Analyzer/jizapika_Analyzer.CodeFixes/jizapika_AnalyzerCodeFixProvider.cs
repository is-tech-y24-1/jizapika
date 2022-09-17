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

namespace jizapika_Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(jizapika_AnalyzerCodeFixProvider)), Shared]
    public class jizapika_AnalyzerCodeFixProvider : CodeFixProvider
    {
        public const string title = "Remove null.";
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
            var node = root.FindNode(diagnosticSpan);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedDocument: async c =>
                    {
                        var returnedExpression = GetReturnedType(node);
                        
                        var updatedRoot = root.ReplaceNode(node, returnedExpression);

                        return document.WithSyntaxRoot(updatedRoot);
                    },
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        private static SyntaxNode GetReturnedType(SyntaxNode node)
        {
            if (node.Parent.IsKind(SyntaxKind.YieldReturnStatement)) return GetYieldReturnedType(node);
            return GetNotYieldReturnedType(node);
        }

        private static SyntaxNode GetNotYieldReturnedType(SyntaxNode node)
        {
            var ancestorsNodes = node.Ancestors().Where(ancestor => ancestor.IsKind(SyntaxKind.MethodDeclaration));

            if (ancestorsNodes.Count() == 0) throw new Exception("This diagnostic isn't on method.");

            var methodNode = ancestorsNodes.FirstOrDefault();

            var returnedGenericOrArrayTypeNodes = methodNode.DescendantNodes().Where(
                descendant => descendant.IsKind(SyntaxKind.GenericName) || descendant.IsKind(SyntaxKind.ArrayType));

            if (returnedGenericOrArrayTypeNodes.Count() == 0) throw new Exception("This method isn't array or generic.");

            var returnedGenericOrArrayTypeNode = returnedGenericOrArrayTypeNodes.FirstOrDefault();

            if (returnedGenericOrArrayTypeNode.IsKind(SyntaxKind.ArrayType))
                return PutInArrayToLowDimension(returnedGenericOrArrayTypeNode);

            if (returnedGenericOrArrayTypeNode.ChildTokens().Where(token => token.Text == "IEnumerable" || token.Text == "List").Count() == 0)
                throw new Exception("This generic isn't IEnumerable or List");

            return SyntaxWithNew(PutInList(TypeInsideCollectionNode(returnedGenericOrArrayTypeNode)));
        }

        private static SyntaxNode GetYieldReturnedType(SyntaxNode node)
        {
            var ancestorsNodes = node.Ancestors().Where(ancestor => ancestor.IsKind(SyntaxKind.MethodDeclaration));

            if (ancestorsNodes.Count() == 0) throw new Exception("This diagnostic isn't on method.");

            var methodNode = ancestorsNodes.FirstOrDefault();

            var IEnumerableOrInheritorTypeNodes = methodNode.DescendantNodes().Where(
                descendant => descendant.IsKind(SyntaxKind.GenericName));

            if (IEnumerableOrInheritorTypeNodes.Count() == 0) throw new Exception("This method isn't IEnumerable or inheritor of IEnumerable.");

            var IEnumerableOrInheritorTypeNode = IEnumerableOrInheritorTypeNodes.FirstOrDefault();

            var returnedGenericOrArrayTypeNodes = IEnumerableOrInheritorTypeNode.ChildNodes().Where(
                childNode => childNode.ChildNodes().Where(
                    childChildNode => childChildNode.IsKind(SyntaxKind.GenericName) || childChildNode.IsKind(SyntaxKind.ArrayType)).Count() != 0);

            if (returnedGenericOrArrayTypeNodes.Count() == 0) throw new Exception("This yield return isn't array or generic.");

            var returnedGenericOrArrayTypeNode = returnedGenericOrArrayTypeNodes.FirstOrDefault().ChildNodes().Where(
                childNode => childNode.IsKind(SyntaxKind.GenericName) || childNode.IsKind(SyntaxKind.ArrayType)).FirstOrDefault();

            if (returnedGenericOrArrayTypeNode.IsKind(SyntaxKind.ArrayType))
                return PutInArrayToLowDimension(returnedGenericOrArrayTypeNode);

            if (returnedGenericOrArrayTypeNode.ChildTokens().Where(token => token.Text == "IEnumerable" || token.Text == "List").Count() == 0)
                throw new Exception("This generic isn't IEnumerable or List");

            return SyntaxWithNew(PutInList(TypeInsideCollectionNode(returnedGenericOrArrayTypeNode)));
        }

        private static ObjectCreationExpressionSyntax SyntaxWithNew(TypeSyntax node)
        {
            return SyntaxFactory.ObjectCreationExpression(node)
                .WithArgumentList(
                    SyntaxFactory.ArgumentList()
                )
                .NormalizeWhitespace();
        }

        private static SyntaxNode PutInArrayToLowDimension(SyntaxNode arrayNode)
        {
            var predefinedTypes = arrayNode.ChildNodes().Where(node => node.IsKind(SyntaxKind.PredefinedType));

            if (predefinedTypes.Count() != 1) throw new Exception("There is not correct array node.");

            var predefinedType = predefinedTypes.First();

            var specifiers = arrayNode.ChildNodes().Where(node => node.IsKind(SyntaxKind.ArrayRankSpecifier));

            if (specifiers.Count() == 0) throw new Exception("Array without specifiers.");

            var newSpecifiers = new List<SyntaxNode>();

            for (int i = 0; i < specifiers.Count() - 1; i++)
            {
                newSpecifiers.Add(specifiers.ElementAt(i));
            }

            return
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("Array"),
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("Empty")
                            )
                            .WithTypeArgumentList(
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                        SyntaxFactory.ArrayType(
                                            SyntaxFactory.IdentifierName(predefinedType.ToString())
                                        )
                                        .WithRankSpecifiers(
                                            SyntaxFactory.List(
                                                newSpecifiers
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                .NormalizeWhitespace();
        }

        private static TypeSyntax PutInList(string identifierName)
        {
            return
                SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier("List")
                )
                .WithTypeArgumentList(
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                            SyntaxFactory.IdentifierName(identifierName)
                        )
                    )
                )
                .NormalizeWhitespace();
        }

        private static SyntaxNode TypeInsideIEnumerableNode(SyntaxNode collectionNode)
        {
            if (collectionNode.IsKind(SyntaxKind.GenericName))
                return collectionNode.ChildNodes().First();

            throw new Exception("It isn't IEnumerable.");
        }

        private static string TypeInsideCollectionNode(SyntaxNode collectionNode)
        {
            if (collectionNode.IsKind(SyntaxKind.GenericName))
                return DeleteCornerBrackets(collectionNode.ChildNodes().First().ToString());

            if (collectionNode.IsKind(SyntaxKind.ArrayType))
                return collectionNode.ChildNodes().Where(node => !node.IsKind(SyntaxKind.ArrayRankSpecifier)).First().ToString();

            throw new Exception("It isn't collection.");
        }

        private static string DeleteCornerBrackets(string text)
        {
            return text.Substring(1, text.Length - 2);
        }
    }
}