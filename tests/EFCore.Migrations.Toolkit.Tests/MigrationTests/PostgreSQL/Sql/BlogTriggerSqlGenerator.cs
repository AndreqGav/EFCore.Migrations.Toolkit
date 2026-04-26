using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.Toolkit.Tests.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Migrations.Toolkit.Tests.MigrationTests.PostgreSQL.Sql;

/// <summary>
/// Генерирует тело триггера для сущности Blog, используя
/// реальные имена колонок и таблицы из модели EF.
/// </summary>
public class BlogTriggerSqlGenerator : CustomSqlGenerator
{
    public BlogTriggerSqlGenerator(DbContext dbContext, ModelBuilder modelBuilder) : base(dbContext, modelBuilder)
    {
    }

    public string GenerateTriggerBody()
    {
        var blogTable = GetTableName<Blog>();
        var nameColumn = GetColumnName<Blog>(x => x.Name);
        var urlColumn = GetColumnName<Blog>(x => x.Url);

        return
            $"IF NEW.{urlColumn} IS NOT NULL AND NEW.{urlColumn} IS DISTINCT FROM OLD.{urlColumn} THEN\n" +
            $"    RAISE EXCEPTION 'Нельзя менять URL';\n" +
            $"END IF;\n" +
            $"IF NEW.{nameColumn} IS NOT NULL THEN\n" +
            $"    UPDATE {blogTable} SET {urlColumn} = NEW.{urlColumn}\n" +
            $"    WHERE {nameColumn} = NEW.{nameColumn};\n" +
            $"END IF;";
    }
}