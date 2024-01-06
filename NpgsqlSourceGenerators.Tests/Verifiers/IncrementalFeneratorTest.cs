using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Model;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace NpgsqlSourceGenerators.Tests.Verifiers;

public sealed class IncrementalGeneratorTest<TIncrementalGenerator> : CSharpSourceGeneratorTest<EmptySourceGeneratorProvider, XUnitVerifier>
    where TIncrementalGenerator : IIncrementalGenerator, new()
{
    private static LanguageVersion LanguageVersion => LanguageVersion.Default;

    protected override IEnumerable<ISourceGenerator> GetSourceGenerators() =>
        new[] { new TIncrementalGenerator().AsSourceGenerator() };

    protected override CompilationOptions CreateCompilationOptions()
    {
        var compilationOptions = base.CreateCompilationOptions();
        return compilationOptions.WithSpecificDiagnosticOptions(
                compilationOptions.SpecificDiagnosticOptions
                .SetItems(VerifierHelper.NullableWarnings));
    }

    protected override ParseOptions CreateParseOptions() =>
        ((CSharpParseOptions)base.CreateParseOptions())
            .WithLanguageVersion(LanguageVersion);

    protected override async Task RunImplAsync(CancellationToken cancellationToken)
    {            
        var testState = TestState
            .WithInheritedValuesApplied(null, ImmutableArray<string>.Empty);

        var diagnostics = await VerifySourceGeneratorAsync(
            testState,
            Verify,
            cancellationToken)
            .ConfigureAwait(false);

        await VerifyDiagnosticsAsync(
            new EvaluatedProjectState(testState, ReferenceAssemblies)
                .WithAdditionalDiagnostics(diagnostics),
            testState.AdditionalProjects.Values
                .Select(additionalProject =>
                    new EvaluatedProjectState(additionalProject, ReferenceAssemblies))
                .ToImmutableArray(),
            Array.Empty<DiagnosticResult>(),
            Verify.PushContext("Diagnostics of test state"),
            cancellationToken)
            .ConfigureAwait(false);
    }
}