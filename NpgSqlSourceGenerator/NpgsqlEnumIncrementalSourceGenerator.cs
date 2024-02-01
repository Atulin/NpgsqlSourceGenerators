using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NpgSqlSourceGenerator;

[Generator]
public class NpgsqlEnumIncrementalSourceGenerator : IIncrementalGenerator
{
    private const string Namespace = "NpgSqlGenerators";
    private const string AttributeName = "PostgresEnumAttribute";

    private const string AttributeSourceCode = // lang=cs
        $$"""
          // <auto-generated/>
          #nullable enable
          namespace {{Namespace}};
                                                   
          /// <summary>
          /// Marks the enum to be used by NpgSqlSourceGenerator
          /// </summary>
          [System.AttributeUsage(System.AttributeTargets.Enum)]
          public class {{AttributeName}} : System.Attribute
          {
              /// <summary>
              /// Sets name to be used in the database.
              /// Used in
              /// <code lang="cs">
              /// (NpgsqlDataSourceBuilder).MapEnum&lt;TEnum&gt;(pgName: {NAME})
              /// (ModelBuilder).HasPostgresEnum&lt;TEnum&gt;(name: {NAME})
              /// </code>
              /// </summary>
              public string? Name { get; set; }
          }
          """;

    private record struct EnumData(string Name, string Namespace, bool IsGlobalNamespace, string? PgName);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute to the compilation.
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "PostgresEnumAttribute.g.cs",
            SourceText.From(AttributeSourceCode, Encoding.UTF8)));

        var p = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"{Namespace}.{AttributeName}",
            (sn, _) => sn is EnumDeclarationSyntax,
            (gasc, _) => new EnumData(
                gasc.TargetSymbol.Name,
                gasc.TargetSymbol.ContainingNamespace.ToDisplayString(),
                gasc.TargetSymbol.ContainingNamespace.IsGlobalNamespace,
                gasc.Attributes[0].NamedArguments.FirstOrDefault(kv => kv.Key == "Name").Value.Value as string
            )
        );

        // Generate the source code.
        context.RegisterSourceOutput(p.Collect(), GenerateCode);
    }


    /// <summary>
    /// Generate code action.
    /// It will be executed on specific nodes (EnumDeclarationSyntax annotated with the [PostgresEnum] attribute) changed by the user.
    /// </summary>
    /// <param name="context">Source generation context used to add source files.</param>
    /// <param name="enumDeclarations">Nodes annotated with the [PostgresEnum] attribute that trigger the generate action.</param>
    private static void GenerateCode(SourceProductionContext context, ImmutableArray<EnumData> enumDeclarations)
    {
        var code = // lang=cs
            $$"""
              // <auto-generated/>
              #nullable enable
              using Npgsql;
              using Microsoft.EntityFrameworkCore;
                     
              namespace {{Namespace}};
              
              internal static class PostgresEnumHelpers
              {
                  /// <summary>
                  /// Calls <see cref="Npgsql.NpgsqlDataSourceBuilder.MapEnum{T}"/> on selected enums
                  /// </summary>
                  public static NpgsqlDataSourceBuilder MapPostgresEnums(this NpgsqlDataSourceBuilder builder)
                  {
              {{string.Join("\n", enumDeclarations.Select(m => BuildMapEnum(m, 8)))}}
                      return builder;
                  }
              
                  /// <summary>
                  /// Calls <see cref="Microsoft.EntityFrameworkCore.NpgsqlModelBuilderExtensions.HasPostgresEnum{T}"/> on selected enums
                  /// </summary>
                  public static void RegisterPostgresEnums(this ModelBuilder builder)
                  {
              {{string.Join("\n", enumDeclarations.Select(m => BuildHasEnum(m, 8)))}}
                  }
              }
              """;

        context.AddSource("PostgresEnumHelpers.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private static string BuildMapEnum(EnumData model, int indent)
    {
        var ns = model.IsGlobalNamespace
            ? $"global::{model.Name}"
            : $"{model.Namespace}.{model.Name}";

        var sb = new StringBuilder()
            .Append(' ', indent)
            .Append("builder.MapEnum<").Append(ns).Append(">(").Append(model.PgName is { } pn ? $"pgName: \"{pn}\"" : null).Append(");");
        return sb.ToString();
    }

    private static string BuildHasEnum(EnumData model, int indent)
    {
        var ns = model.IsGlobalNamespace
            ? $"global::{model.Name}"
            : $"{model.Namespace}.{model.Name}";

        var sb = new StringBuilder()
            .Append(' ', indent)
            .Append("builder.HasPostgresEnum<").Append(ns).Append(">(").Append(model.PgName is { } pn ? $"name: \"{pn}\"" : null)
            .Append(");");
        return sb.ToString();
    }
}