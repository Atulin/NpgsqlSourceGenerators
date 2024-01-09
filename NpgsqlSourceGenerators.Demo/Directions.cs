using NpgSqlGenerators;

namespace NpgsqlSourceGenerators.Demo;

[PostgresEnum]
enum Directions
{
	North,
	West,
	South,
	East
}
