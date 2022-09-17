using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading;
using System.Linq;

namespace jizapika_Analyzer.Test
{
    [TestClass]
    public class jizapika_AnalyzerUnitTest
    {
        // fixed smth:
        [TestMethod]
        public async Task IEnumerableMethodWithReturnedNull_ReturnedNewListInsteadOfNull()
        {
            var test = @"
                using System.Collections.Generic;
                class A
                {
                    public IEnumerable<int> MakeIEnumerable()
                    {
                        return null;
                    }
                }";

            var fixtest = @"
                using System.Collections.Generic;
                class A
                {
                    public IEnumerable<int> MakeIEnumerable()
                    {
                        return new List<int>();
                    }
                }";

            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task ListMethodWithReturnedNull_ReturnedNewListInsteadOfNull()
        {
            var test = @"
                using System.Collections.Generic;
                class A
                {
                    public List<int> MakeList()
                    {
                        return null;
                    }
                }";

            var fixtest = @"
                using System.Collections.Generic;
                class A
                {
                    public List<int> MakeList()
                    {
                        return new List<int>();
                    }
                }";

            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task ArrayMethodWithReturnedNull_ReturnedNewEmptyArrayInsteadOfNull()
        {
            var test = @"
                using System.Collections.Generic;
                class A
                {
                    public int[] MakeArray()
                    {
                        return null;
                    }
                }";

            var fixtest = @"
                using System.Collections.Generic;
                class A
                {
                    public int[] MakeArray()
                    {
                        return Array.Empty<int>();
                    }
                }";
            
            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task IEnumerableYieldCollectionWithReturnedNull_ReturnedNewListInsteadOfNull()
        {
            var test = @"
                using System.Collections.Generic;
                class A
                {
                    public IEnumerable<List<int>> MakeLists()
                    {
                        yield return null;
                    }
                }";

            var fixtest = @"
                using System.Collections.Generic;
                class A
                {
                    public IEnumerable<List<int>> MakeLists()
                    {
                        yield return new List<int>();
                    }
                }";

            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task ListYieldCollectionWithReturnedNull_ReturnedNewListInsteadOfNull()
        {
            var test = @"
                using System.Collections.Generic;
                class A
                {
                    public List<int[]> MakeArrays()
                    {
                        yield return null;
                    }
                }";

            var fixtest = @"
                using System.Collections.Generic;
                class A
                {
                    public List<int[]> MakeArrays()
                    {
                        yield return Array.Empty<int>();
                    }
                }";

            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task AlternativeListMethodWithReturnedNull_ReturnedNewListInsteadOfNull()
        {
            var test = @"
                using System.Collections.Generic;
                class A
                {
                    public List<Ienumerable<Dictionary<int, Cat>>> MakeList()
                    {
                        return null;
                    }
                }";

            var fixtest = @"
                using System.Collections.Generic;
                class A
                {
                    public List<Ienumerable<Dictionary<int, Cat>>> MakeList()
                    {
                        return new List<Ienumerable<Dictionary<int, Cat>>>();
                    }
                }";

            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task AlternativeYieldListMethodWithReturnedNull_ReturnedNewListInsteadOfNull()
        {
            var test = @"
                using System.Collections.Generic;
                class A
                {
                    public Ienumerable<List<Ienumerable<Dictionary<(int, float), Cat>>>> MakeList()
                    {
                        yield return null;
                    }
                }";

            var fixtest = @"
                using System.Collections.Generic;
                class A
                {
                    public Ienumerable<List<Ienumerable<Dictionary<(int, float), Cat>>>> MakeList()
                    {
                        yield return new List<Ienumerable<Dictionary<(int, float), Cat>>>();
                    }
                }";

            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task TwoDimensionalArray_ReturnedNewTwoDimensionalArrayInsteadOfNull()
        {
            var test = @"
                using System.Collections.Generic;
                class A
                {
                    public int[][] MakeTwoDimensionalArray()
                    {
                        return null;
                    }
                }";

            var fixtest = @"
                using System.Collections.Generic;
                class A
                {
                    public int[][] MakeTwoDimensionalArray()
                    {
                        return Array.Empty<int[]>();
                    }
                }";

            await RunTest(test, fixtest);
        }

        private static async Task RunTest(string code, string expectedChangedCode)
        {
            var (diagnostics, document, workspace) = await Utilities.GetDiagnosticsAdvanced(code);

            // не знаю, почему там больше одной диагностики получается, выбираем только нужный анализатор.
            //throw new Exception(diagnostics.Count().ToString());
            var analyzerDiagnostics = diagnostics.Where(diag => diag.Id == "jizapika_AnalyzerAnalyzer");

            if (analyzerDiagnostics.Count() != 1) throw new Exception("Not correct analyzer on test.");

            var analyzerDiagnostic = analyzerDiagnostics.First();

            var codeFixProvider = new jizapika_AnalyzerCodeFixProvider();

            CodeAction registeredCodeAction = null;

            var context = new CodeFixContext(document, analyzerDiagnostic, (codeAction, _) =>
            {
                if (registeredCodeAction != null)
                    throw new Exception("Code action was registered more than once");

                registeredCodeAction = codeAction;

            }, CancellationToken.None);

            await codeFixProvider.RegisterCodeFixesAsync(context);

            if (registeredCodeAction == null)
                throw new Exception("Code action was not registered");

            var operations = await registeredCodeAction.GetOperationsAsync(CancellationToken.None);

            foreach (var operation in operations)
            {
                operation.Apply(workspace, CancellationToken.None);
            }

            var updatedDocument = workspace.CurrentSolution.GetDocument(document.Id);


            var newCode = (await updatedDocument.GetTextAsync()).ToString();

            Assert.AreEqual(expectedChangedCode, newCode);
        }
    }
}
