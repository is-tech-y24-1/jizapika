using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace jizapika_Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class jizapika_AnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "jizapika_AnalyzerAnalyzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "ReturnedNullAnalyzer";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

         public override void Initialize(AnalysisContext context)
         {
             context.RegisterSyntaxNodeAction(c =>
             {
                 var nullCollectionTokens = c.Node.DescendantTokens().Where(
                     token => token.IsKind(SyntaxKind.NullKeyword) && token.Text == "null" && IsCollectionMethod(token));

                 if (nullCollectionTokens.Count() > 0)
                 {
                     var nullToken = nullCollectionTokens.FirstOrDefault();

                     var diag = Diagnostic.Create(Rule, nullToken.GetLocation(), nullToken.Text);
                     c.ReportDiagnostic(diag);
                 }
             },
             SyntaxKind.ReturnStatement);

             context.RegisterSyntaxNodeAction(c =>
             {
                 var nullCollectionTokens = c.Node.DescendantTokens().Where(
                     token => token.IsKind(SyntaxKind.NullKeyword) && token.Text == "null" && IsGenericCollectionMethod(token));

                 if (nullCollectionTokens.Count() > 0)
                 {
                     var nullToken = nullCollectionTokens.FirstOrDefault();

                     var diag = Diagnostic.Create(Rule, nullToken.GetLocation(), nullToken.Text);
                     c.ReportDiagnostic(diag);
                 }
             },
             SyntaxKind.YieldReturnStatement);
         }

         private bool IsCollectionMethod(SyntaxToken initialToken)
         {
             var ancestorsNodes = initialToken.Parent.Ancestors().Where(node => node.IsKind(SyntaxKind.MethodDeclaration));

             if (ancestorsNodes.Count() == 0) return false;

             var methodNode = ancestorsNodes.First();

             var returnedGenericOrArrayTypeNodes = methodNode.DescendantNodes().Where(
                 node => node.IsKind(SyntaxKind.GenericName) || node.IsKind(SyntaxKind.ArrayType));

             if (returnedGenericOrArrayTypeNodes.Count() == 0) return false;

             var returnedGenericOrArrayTypeNode = returnedGenericOrArrayTypeNodes.FirstOrDefault();

             if (returnedGenericOrArrayTypeNode.IsKind(SyntaxKind.ArrayType)) return true;

             return IsListOrIEnumerable(returnedGenericOrArrayTypeNode);
         }

         private bool IsGenericCollectionMethod(SyntaxToken initialToken)
         {
             var methodNodes = initialToken.Parent.Ancestors().Where(node => node.IsKind(SyntaxKind.MethodDeclaration));

             if (methodNodes.Count() == 0) return false;

             var genericOrArrayTypeCollectionNodes = methodNodes.First().DescendantNodes().Where(node =>
             {
                 if (!node.IsKind(SyntaxKind.GenericName)) return false;

                 return !(node.ChildNodes().Where(
                     childNode => childNode.ChildNodes().Where(
                         childChildNode => childChildNode.IsKind(SyntaxKind.GenericName) || childChildNode.IsKind(SyntaxKind.ArrayType)
                         )
                         .Count() != 0
                     )
                     .Count() == 0);
             });

             if (genericOrArrayTypeCollectionNodes.Count() == 0) return false;

             var genericOrArrayTypeNodes = genericOrArrayTypeCollectionNodes.First().DescendantNodes().Where(
                 node => node.IsKind(SyntaxKind.GenericName) || node.IsKind(SyntaxKind.ArrayType));

             if (genericOrArrayTypeNodes.Count() == 0) return false;

             var genericOrArrayTypeNode = genericOrArrayTypeNodes.First();

             if (genericOrArrayTypeNode.IsKind(SyntaxKind.ArrayType)) return true;

             return IsListOrIEnumerable(genericOrArrayTypeNode);
         }

         private bool IsListOrIEnumerable(SyntaxNode genericNode)
         {
             return genericNode.ChildTokens().Where(
                     token => token.Text == "IEnumerable" || token.Text == "List").Count() > 0;
         }

        /*public override void Initialize(AnalysisContext context)
        {
            throw new System.Exception("Blablabla");
            context.RegisterSyntaxNodeAction(c =>
            {
                var methodNode = c.Node;

                var predefinedNodes = methodNode.ChildNodes().Where(
                    node => node.IsKind(SyntaxKind.PredefinedType));

                if (predefinedNodes.Count() != 0)
                {
                    var predefinedNode = predefinedNodes.First();

                    var predefinedNodeBools = predefinedNode.ChildTokens().Where(
                        token => token.IsKind(SyntaxKind.BoolKeyword));

                    if (predefinedNodeBools.Count() != 0) return;
                }

                var tryNameTokens = methodNode.ChildTokens().Where(
                    token => IsTryName(token.ToString()));

                if (tryNameTokens.Count() > 0)
                {

                    var tryNameToken = tryNameTokens.FirstOrDefault();

                    var diag = Diagnostic.Create(Rule, tryNameToken.GetLocation(), tryNameToken.Text);
                    c.ReportDiagnostic(diag);
                }
            },
            SyntaxKind.MethodDeclaration);
        }

        private static bool IsTryName(string name)
        {
            return name.Length >= 3 && name[0] == 'T' && name[1] == 'r' && name[2] == 'y';
        }*/
    }
}