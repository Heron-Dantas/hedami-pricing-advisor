# Calculator verification — October 24, 2026

## Observed results

- Console demo completed successfully (exit 0), using only synthetic inputs and no LLM.
- Production cost: BRL 24.6667 displayed; raw decimal precision retained internally.
- 30% target: list price BRL 44.45; after 10% discount BRL 40.01; resulting contribution margin 23.35%.
- Domain and test assemblies compiled successfully.
- Standard `dotnet test` did **not** complete: VSTest could not bind a local socket (`SocketException (13): Permission denied`). Initial parallel MSBuild also encountered restricted named pipes. These are environment failures, not passing test runs.
- A temporary in-process reflection driver invoked the existing test methods, including all InlineData rows, with the actual xUnit assertions: **16 cases passed**, exit 0. No assertions were replaced. This is fallback verification, not a successful VSTest run.
- NuGet reported NU1900: vulnerability metadata could not be retrieved. Package restoration succeeded using locally available dependencies; dependency vulnerability status was not verified.
- `git init` failed because writes to `.git` are prohibited by this session. No Git repository/checkpoint or identity configuration was created.

## Reproduce normal verification

```sh
dotnet test --nologo -m:1 -p:UseSharedCompilation=false -nodeReuse:false
dotnet run --project src/Hedami.PricingAdvisor.Console --no-restore -m:1 -p:UseSharedCompilation=false -nodeReuse:false
```

Run standard tests in a terminal that permits local test-runner sockets. The fallback used a temporary net10.0 console project referencing the compiled test output DLLs, then `dotnet run` with single-node MSBuild and shared compilation disabled. Its complete driver source is preserved below for auditability:

```csharp
using System.Reflection;
using System.Globalization;
using Hedami.PricingAdvisor.Domain.Tests;
var instance = new PricingCalculatorTests();
var passed = 0;
foreach (var method in typeof(PricingCalculatorTests).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
{
    var rows = method.GetCustomAttributesData().Where(a => a.AttributeType.Name == "InlineDataAttribute").ToArray();
    if (rows.Length == 0)
    {
        method.Invoke(instance, null);
        passed++;
    }
    foreach (var row in rows)
    {
        var values = (IReadOnlyCollection<CustomAttributeTypedArgument>)row.ConstructorArguments[0].Value!;
        var parameters = method.GetParameters();
        var argsForTest = values.Select((v, i) => Convert.ChangeType(v.Value, parameters[i].ParameterType, CultureInfo.InvariantCulture)).ToArray();
        method.Invoke(instance, argsForTest);
        passed++;
    }
}
Console.WriteLine($"PASS: {passed} cases executed directly with xUnit assertions. This is an in-process fallback, not a VSTest run.");
```

No model run, harness evaluation, real business-data validation or reviewer comparison was performed.
