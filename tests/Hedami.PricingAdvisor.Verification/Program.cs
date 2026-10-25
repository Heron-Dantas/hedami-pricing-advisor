using System.Globalization;
using System.Reflection;
using Hedami.PricingAdvisor.Domain.Tests;

var passed = 0;
var failed = 0;
foreach (var type in typeof(PricingCalculatorTests).Assembly.GetTypes().Where(x => x.IsPublic && x.IsClass).OrderBy(x => x.Name))
foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    var attributes = method.GetCustomAttributesData();
    if (!attributes.Any(x => x.AttributeType.Name is "FactAttribute" or "TheoryAttribute")) continue;
    var rows = attributes.Where(x => x.AttributeType.Name == "InlineDataAttribute").ToArray();
    var cases = rows.Length == 0 ? new object?[][] { [] } : rows.Select(row =>
    {
        var values = (IReadOnlyCollection<CustomAttributeTypedArgument>)row.ConstructorArguments[0].Value!;
        var parameters = method.GetParameters();
        return values.Select((v, i) => Convert.ChangeType(v.Value, parameters[i].ParameterType, CultureInfo.InvariantCulture)).ToArray();
    }).ToArray();
    foreach (var values in cases)
    {
        try
        {
            var instance = Activator.CreateInstance(type)!;
            var result = method.Invoke(instance, values);
            if (result is Task task) await task;
            passed++;
            Console.WriteLine($"PASS {type.Name}.{method.Name}");
        }
        catch (Exception ex)
        {
            failed++;
            Console.WriteLine($"FAIL {type.Name}.{method.Name}: {(ex is TargetInvocationException ? ex.InnerException : ex)}");
        }
    }
}
Console.WriteLine($"In-process verification: {passed} passed; {failed} failed. Existing xUnit assertions; not VSTest. No real LLM calls.");
return failed == 0 ? 0 : 1;
