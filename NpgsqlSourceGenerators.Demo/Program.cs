using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using NpgSqlGenerators;

Console.WriteLine("Hello, World!");

var dataSourceBuilder = new NpgsqlDataSourceBuilder();
dataSourceBuilder.MapPostgresEnums();

var modelBuilder = new ModelBuilder();
modelBuilder.RegisterPostgresEnums();

var optionsBuilder = new NpgsqlDbContextOptionsBuilder(new DbContextOptionsBuilder());
optionsBuilder.MapPostgresEnums();


[PostgresEnum(Name = "DoW")]
public enum DaysOfWeek
{
	Monday,	Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
}

[PostgresEnum]
enum Directions
{
	North,
	West,
	South,
	East
}
