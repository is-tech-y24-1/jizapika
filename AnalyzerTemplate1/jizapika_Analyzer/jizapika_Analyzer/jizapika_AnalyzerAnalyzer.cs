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
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
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
        }
    }
}