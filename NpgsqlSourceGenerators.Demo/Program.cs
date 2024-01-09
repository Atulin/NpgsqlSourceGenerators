using NpgSqlEnumGenerators;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

[PostgresEnum]
enum Directions
{
	North,
	West,
	South,
	East
}