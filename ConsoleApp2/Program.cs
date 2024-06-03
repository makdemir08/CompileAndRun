
RuntimeCompiler compiler = new RuntimeCompiler();
Dictionary<string, object> parameters = new Dictionary<string, object>();
parameters.Add("param1", "value1");
parameters.Add("param2", 42);
Dictionary<string, object> results = compiler.CompileAndRunFunction(parameters);
Console.WriteLine("Results:");
foreach (var result in results)
{
    Console.WriteLine($"{result.Key}: {result.Value}");
}

