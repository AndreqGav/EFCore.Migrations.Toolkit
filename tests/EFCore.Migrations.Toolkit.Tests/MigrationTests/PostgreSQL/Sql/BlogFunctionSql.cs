using System;

namespace EFCore.Migrations.Toolkit.Tests.MigrationTests.PostgreSQL.Sql;

/// <summary>
/// SQL-определение пользовательской функции get_blog_name и её удаления.
/// Используется в интеграционном контексте для проверки HasDbFunction.
/// </summary>
public static class BlogFunctionSql
{
    public static string GetName(int id) => throw new InvalidOperationException();

    public static string Up() =>
        "CREATE OR REPLACE FUNCTION GetName(id integer)\n" +
        "RETURNS text AS $$\n" +
        "BEGIN\n" +
        "RETURN (SELECT \"Name\" FROM \"Blogs\" WHERE \"Id\" = id);\n" +
        " END;\n" +
        "$$ LANGUAGE plpgsql;";

    public static string Down() => "DROP FUNCTION IF EXISTS GetName";
}