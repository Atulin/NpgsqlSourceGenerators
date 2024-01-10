using NpgSqlGenerators;

namespace NpgsqlSourceGenerators.Demo;

[PostgresEnum(Name = "DoW")]
public enum DaysOfWeek
{
	Monday,	Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
}