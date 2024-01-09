using VerifyCS = NpgsqlSourceGenerators.Tests.Verifiers.IncrementalGeneratorVerifier<NpgSqlSourceGenerator.NpgsqlEnumIncrementalSourceGenerator>;

namespace NpgsqlSourceGenerators.Tests;

public class UnitTest1
{
    private const string ExpectedAttribute = """
                                             // <auto-generated/>
                                             #nullable enable
                                             namespace NpgSqlGenerators;
                                             
                                             [System.AttributeUsage(System.AttributeTargets.Enum)]
                                             public class PostgresEnumAttribute : System.Attribute
                                             {
                                                 public string? Name { get; set; }
                                             }
                                             """;

    [Fact]
    public async Task GeneratesAttribute()
    {
        const string code = "";

        const string expected = """
                                // <auto-generated/>
                                #nullable enable
                                using Npgsql;
                                using Microsoft.EntityFrameworkCore;
                                
                                namespace NpgSqlGenerators;
                                
                                internal static class PostgresEnumHelpers
                                {
                                    public static NpgsqlDataSourceBuilder MapPostgresEnums(this NpgsqlDataSourceBuilder builder)
                                    {
                                        
                                        return builder;
                                    }
                                
                                    public static void RegisterPostgresEnums(this ModelBuilder builder)
                                    {
                                        
                                    }
                                }
                                """;

        await VerifyCS.VerifyGeneratorAsync(code, ("PostgresEnumAttribute.g.cs", ExpectedAttribute), ("PostgresEnumHelpers.g.cs", expected));
    }
}