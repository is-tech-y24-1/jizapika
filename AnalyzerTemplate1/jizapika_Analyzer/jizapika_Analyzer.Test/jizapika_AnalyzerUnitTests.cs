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
        public async Task EmptyParameterMethodAndTrivialCase_CorrectWork()
        {
            var test =
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public Student TryCreationMe()
    {
        return new Student(string.Empty);
    }
}";

            var fixtest = 
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public bool TryCreationMe(out Student value)
    {
        value = new Student(string.Empty);
        return true;
    }
}";

            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task ManyParametersMethodAndTrivialCase_CorrectWork()
        {
            var test =
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public Student TryCreationB(out Student B, string a)
    {
        B = new Student(a);
        return new Student(a);
    }
}";

            var fixtest =
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public bool TryCreationB(out Student value, out Student B, string a)
    {
        B = new Student(a);
        value = new Student(a);
        return true;
    }
}";

            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task ManyValueParametersMethodAndTrivialCase_CorrectWork()
        {
            var test =
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public Student TryCreationB(string value1, string value)
    {
        return new Student(value1 + value);
    }
}";

            var fixtest =
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public bool TryCreationB(out Student value2, string value1, string value)
    {
        value2 = new Student(value1 + value);
        return true;
    }
}";

            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task MethodWithElseIfConstruction_CorrectWork()
        {
            var test =
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public Student TryCreationB(string salue, string value)
    {
        if (salue != value) return new Student(salue + value);
        else if (salue.Contains(value)) return new Student(salue);
        else if (value.Contains(salue)) return new Student(value);
        else return new Student(String.Empty);
    }
}";

            var fixtest =
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public bool TryCreationB(out Student value1, string salue, string value)
    {
        if (salue != value)
        {
            value1 = new Student(salue + value);
            return true;
        }
        else if (salue.Contains(value))
        {
            value1 = new Student(salue);
            return true;
        }
        else if (value.Contains(salue))
        {
            value1 = new Student(value);
            return true;
        }
        else
        {
            value1 = new Student(String.Empty);
            return true;
        }
    }
}";

            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task MethodWithReturnedLocalVariable_CorrectWork()
        {
            var test =
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public Student TryCreationB(string value1, string value)
    {
        Student a = new Student(value1 + value);
        return a;
    }
}";

            var fixtest =
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public bool TryCreationB(out Student value2, string value1, string value)
    {
        Student a = new Student(value1 + value);
        value2 = a;
        return true;
    }
}";

            await RunTest(test, fixtest);
        }

        [TestMethod]
        public async Task TestWithIntensions_CorrectWork()
        {
            var test =
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public Student TryCreationB(string value1, string value)
    {
        return new Student(value1 + value);
    }
}";

            var fixtest =
@"public class Student
{
    private readonly string _name;
    public Student(string name)
    {
        _name = name;
    }
}

public class TestClass
{
    public bool TryCreationB(out Student value2, string value1, string value)
    {
        value2 = new Student(value1 + value);
        return true;
    }
}";

            await RunTest(test, fixtest);
        }

        private static async Task RunTest(string code, string expectedChangedCode)
        {
            var (diagnostics, document, workspace) = await Utilities.GetDiagnosticsAdvanced(code);

            var analyzerDiagnostics = diagnostics.Where(diag => diag.Id == "jizapika_AnalyzerAnalyzer");

            if (analyzerDiagnostics.Count() == 0) throw new Exception("This text wasn't analyzing.");

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