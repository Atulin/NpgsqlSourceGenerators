using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgSqlGenerators;

Console.WriteLine("Hello, World!");

var dataSourceBuilder = new NpgsqlDataSourceBuilder();
dataSourceBuilder.MapPostgresEnums();

var modelBuilder = new ModelBuilder();
modelBuilder.RegisterPostgresEnums();