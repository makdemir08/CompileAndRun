using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;

public class RuntimeCompiler
{
    public Dictionary<string, object> CompileAndRunFunction(Dictionary<string, object> parameters)
    {
        //Dinamik kod örneği
        string code = @"
            using System;
            using System.Collections.Generic;

            public class RuntimeFunction
            {
                public Dictionary<string, object> MainFunction(Dictionary<string, object> parameters)
                {
                    Dictionary<string, object> results = new Dictionary<string, object>();

                    object paramValue;
                    foreach (var parameter in parameters)
                    {
                        string paramName = parameter.Key;
                        if(parameter.Value is int){
                            paramValue = (int)parameter.Value+5;
                        }
                        else
                        {
                            paramValue = ReverseString((string)parameter.Value);
                        }

                        results.Add(paramName, paramValue);
                    }

                    return results;
                }

                private string ReverseString(string input)
                {
                    char[] charArray = input.ToCharArray();
                    Array.Reverse(charArray);
                    return new string(charArray);
                }
            }";

        // C# kodunu derle
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        string assemblyName = Path.GetRandomFileName();
        MetadataReference[] references = { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };
        CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, syntaxTrees: new[] { syntaxTree }, references: references, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using (MemoryStream ms = new MemoryStream())
        {
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                foreach (Diagnostic diagnostic in failures)
                {
                    Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                }

                return null;
            }
            else
            {
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());
                Type runtimeFunctionType = assembly.GetType("RuntimeFunction");
                object runtimeFunctionInstance = Activator.CreateInstance(runtimeFunctionType);

                MethodInfo mainFunctionMethod = runtimeFunctionType.GetMethod("MainFunction");
                object functionResult = mainFunctionMethod.Invoke(runtimeFunctionInstance, new object[] { parameters });

                return (Dictionary<string, object>)functionResult;
            }
        }
    }
}
