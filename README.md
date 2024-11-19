[![NuGet Version](https://img.shields.io/nuget/v/Atulin.NpgsqlSourceGenerator?style=for-the-badge)](https://www.nuget.org/packages/Atulin.NpgSqlSourceGenerator/)
![NuGet Downloads](https://img.shields.io/nuget/dt/Atulin.NpgsqlSourceGenerator?style=for-the-badge)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Atulin/NpgsqlSourceGenerators/nuget.yml?style=for-the-badge)
[![GitHub License](https://img.shields.io/github/license/Atulin/NpgsqlSourceGenerators?style=for-the-badge)](/LICENSE)

# Npgsql Source Generators

Registering all enums one by one is tedious. Use this.

## Usage

Place `[PostgresEnum]` attribute on the enums you want to register...

```cs
namespace MyCoolApp;

[PostgresEnum]
public enum Status {
    Completed,
    InProgress,
    Started,
    Queued
}

[PostgresEnum(Name = "process_priority")]
public enum Priority {
    High,
    Medium,
    Low
}
```

...and register them

```cs
var source = new NpgsqlDataSourceBuilder(connectionString).npgSourceBuilder
    .MapPostgresEnums()
    .Build();
services.AddDbContext<MyDbContext>(options => options.UseNpgsql(source));
```
```cs
public class MyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.RegisterPostgresEnums();
    }    
}
```
> [!TIP]
> In Npgsql 9.0, you can use *just* this instead of the previous two
```cs
builder.Services.AddDbContext<MyContext>(options => options.UseNpgsql(
    "<connection string>",
    o => o.MapPostgresEnums()));
```
