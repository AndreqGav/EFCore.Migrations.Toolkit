using EFCore.Migrations.CustomSql.Abstractions;
using EFCore.Migrations.Toolkit.Tests.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Migrations.Toolkit.Tests.MigrationTests.PostgreSQL.Sql
{
    /// <summary>
    /// Генерирует SQL для представления blog_names, используя
    /// реальные имена колонок и таблицы из модели EF.
    /// </summary>
    public class BlogViewSqlGenerator : CustomSqlGenerator
    {
        public BlogViewSqlGenerator(DbContext dbContext, ModelBuilder modelBuilder)
            : base(dbContext, modelBuilder)
        {
        }

        public string Create()
        {
            var blogTable = GetTableName<Blog>();
            var idColumn = GetColumnName<Blog>(e => e.Id);
            var nameColumn = GetColumnName<Blog>(e => e.Name);

            return
                "CREATE OR REPLACE VIEW public.blog_names\n" +
                $"AS SELECT {idColumn}, {nameColumn} FROM {blogTable}";
        }

        public string Drop() => "DROP VIEW IF EXISTS public.blog_names";
    }
}