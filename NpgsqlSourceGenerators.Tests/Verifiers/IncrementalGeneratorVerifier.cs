using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace NpgsqlSourceGenerators.Tests.Verifiers;

internal static class IncrementalGeneratorVerifier<TIncrementalGenerator>
    where TIncrementalGenerator : IIncrementalGenerator, new()
{
    internal static readonly (string filename, string content)[] emptyGeneratedSources = Array.Empty<(string filename, string content)>();

    public static async Task VerifyGeneratorAsync(string source, (string filename, string content) generatedSource)
        => await VerifyGeneratorAsync(source, [generatedSource]);

    public static async Task VerifyGeneratorAsync(string source, params (string filename, string content)[] generatedSources)
    {
        var test = new IncrementalGeneratorTest<TIncrementalGenerator>
        {
            TestState =
            {
                Sources = { source },
            },
            ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
        };

        foreach (var (filename, content) in generatedSources)
        {
            test.TestState.GeneratedSources.Add((typeof(TIncrementalGenerator), filename, SourceText.From(
                // Replace line endings because CodeStringBuilder uses the environment's newline.
                content.ReplaceLineEndings(),
                Encoding.UTF8)));
        }

        await test.RunAsync(CancellationToken.None);
    }
}