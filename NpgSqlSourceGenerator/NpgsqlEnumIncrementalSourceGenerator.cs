using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NpgSqlSourceGenerator;

/// <summary>
/// A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
/// When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator]
public class NpgsqlEnumIncrementalSourceGenerator : IIncrementalGenerator
{
	private const string Namespace = "NpgSqlGenerators";
	private const string AttributeName = "PostgresEnumAttribute";

	private const string AttributeSourceCode = $$"""
	    // <auto-generated/>
	    #nullable enable
	    namespace {{Namespace}};
	                                             
	    [System.AttributeUsage(System.AttributeTargets.Enum)]
	    public class {{AttributeName}} : System.Attribute
	    {
	        public string? Name { get; set; }
	    }
	    """;

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Add the marker attribute to the compilation.
		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"PostgresEnumAttribute.g.cs",
			SourceText.From(AttributeSourceCode, Encoding.UTF8)));

		// Filter classes annotated with the [PostgresEnum] attribute. Only filtered Syntax Nodes can trigger code generation.
		//var provider = context.SyntaxProvider
		//	.CreateSyntaxProvider(
		//		(s, _) => s is EnumDeclarationSyntax,
		//		(ctx, _) => GetEnumDeclarationForSourceGen(ctx))
		//	.Where(t => t.pgEnumAttributeFound)
		//	.Select((t, _) => t.enumDeclarationSyntax);

		var p = context.SyntaxProvider.ForAttributeWithMetadataName(
			 $"{Namespace}.{AttributeName}",
			 (sn, _) => sn is EnumDeclarationSyntax,
			 (gasc, _) => (Namespace: gasc.TargetSymbol.ContainingNamespace.ToDisplayString(), gasc.TargetSymbol.Name)
		);

		// Generate the source code.
		context.RegisterSourceOutput(context.CompilationProvider.Combine(p.Collect()),
			(ctx, t) => GenerateCode(ctx, t.Left, t.Right));
	}

	/// <summary>
	/// Checks whether the Node is annotated with the [Report] attribute and maps syntax context to the specific node type (ClassDeclarationSyntax).
	/// </summary>
	/// <param name="context">Syntax context, based on CreateSyntaxProvider predicate</param>
	/// <returns>The specific cast and whether the attribute was found.</returns>
	private static (EnumDeclarationSyntax enumDeclarationSyntax, bool pgEnumAttributeFound) GetEnumDeclarationForSourceGen(
		GeneratorSyntaxContext context)
	{
		var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

		// Go through all attributes of the class.
		foreach (var attributeSyntax in enumDeclarationSyntax.AttributeLists.SelectMany(attributeListSyntax => attributeListSyntax.Attributes))
		{
			if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not {} attributeSymbol)
			{
				continue; // if we can't get the symbol, ignore it
			}

			var attributeName = attributeSymbol.ContainingType.ToDisplayString();
			
			// Check the full name of the [Report] attribute.
			if (attributeName == $"{Namespace}.{AttributeName}")
			{
				return (enumDeclarationSyntax, true);
			}
		}

		return (enumDeclarationSyntax, false);
	}

	/// <summary>
	/// Generate code action.
	/// It will be executed on specific nodes (ClassDeclarationSyntax annotated with the [Report] attribute) changed by the user.
	/// </summary>
	/// <param name="context">Source generation context used to add source files.</param>
	/// <param name="compilation">Compilation used to provide access to the Semantic Model.</param>
	/// <param name="enumDeclarations">Nodes annotated with the [Report] attribute that trigger the generate action.</param>
	private static void GenerateCode(SourceProductionContext context, Compilation compilation,
		ImmutableArray<(string Namespace, string Name)> enumDeclarations)
	{
		//var models = enumDeclarations
		//	.Select(ed => (enumDeclarationSyntax: ed, semanticModel: compilation.GetSemanticModel(ed.SyntaxTree)))
		//	.Select(sm => sm.semanticModel.GetDeclaredSymbol(sm.enumDeclarationSyntax))
		//	.Cast<INamedTypeSymbol>()
		//	.Select(its => (ns: its.ContainingNamespace.ToDisplayString(), its.Name))
		//	.ToArray();

		var code = $$"""
		    // <auto-generated/>
		    #nullable enable
		    using Npgsql;
		    using Microsoft.EntityFrameworkCore;
		           
		    namespace {{Namespace}};
		           
		    internal static class PostgresEnumHelpers
		    {
		        public static NpgsqlDataSourceBuilder MapPostgresEnums(this NpgsqlDataSourceBuilder builder)
		        {
		            {{string.Join("\n    ", enumDeclarations.Select(m => $"builder.MapEnum<{m.Namespace}.{m.Name}>();"))}}
		            return builder;
		        }

		        public static void RegisterPostgresEnums(this ModelBuilder builder)
		        {
		            {{string.Join("\n    ", enumDeclarations.Select(m => $"builder.HasPostgresEnum<{m.Namespace}.{m.Name}>();"))}}
		        }
		    }
		    """;
		
		context.AddSource("PostgresEnumHelpers.g.cs", SourceText.From(code, Encoding.UTF8));
	}
}