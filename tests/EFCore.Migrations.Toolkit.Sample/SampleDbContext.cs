using EFCore.Migrations.CustomSql;
using EFCore.Migrations.Toolkit.Sample.Models;
using EFCore.Migrations.Triggers;
using EFCore.Migrations.Triggers.PostgreSQL.Enums;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Migrations.Toolkit.Sample;

/// <summary>
/// Пример DbContext, демонстрирующий все возможности EFCore.Migrations.Toolkit:
/// автокомментарии, пользовательский SQL и триггеры.
/// </summary>
public class SampleDbContext : DbContext
{
    public DbSet<Post> Posts { get; set; } = null!;

    public DbSet<Author> Authors { get; set; } = null!;

    public SampleDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        const string connectionString = "Host=localhost;Port=5432;Database=SampleDb;Username=postgres;Password=your_password";

        optionsBuilder.UseNpgsql(connectionString, o => o.UseTriggers())
            .UseCustomSql()
            .UseAutoComments(o => o.AddEnumDescriptions());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // CustomSql: представление для опубликованных публикаций.
        // UP-скрипт выполнится при применении миграции, DOWN — при откате.
        modelBuilder.AddCustomSql(
            "published_posts_view",
            """
            CREATE OR REPLACE VIEW published_posts_view AS
            SELECT p."Id", p."Title", p."AuthorId"
            FROM "Posts" p
            WHERE p."Status" = 1
            """,
            "DROP VIEW IF EXISTS published_posts_view");

        // CustomSql: PostgreSQL-функция для получения имени автора по идентификатору.
        modelBuilder.AddCustomSql(
            "get_author_name",
            """
            CREATE OR REPLACE FUNCTION get_author_name(author_id integer)
            RETURNS text
            LANGUAGE sql STABLE AS $$
                SELECT "Name" FROM "Authors" WHERE "Id" = author_id;
            $$
            """,
            "DROP FUNCTION IF EXISTS get_author_name(integer)");

        modelBuilder.Entity<Post>(entity =>
        {
            // Triggers: сброс статуса в Draft при вставке новой записи.
            entity.BeforeInsert(
                "set_post_status_on_insert",
                """NEW."Status" = 0;""");

            // Triggers: запрет архивирования через UPDATE прямо в триггере.
            entity.BeforeUpdate(
                "prevent_direct_archive",
                """
                IF NEW."Status" = 2 AND OLD."Status" = 1 THEN
                    RAISE EXCEPTION 'Use dedicated archive procedure instead of direct UPDATE';
                END IF;
                """);

            // Triggers (PostgreSQL): CONSTRAINT-триггер после вставки.
            // Deferrable — выполняется в конце транзакции, а не сразу.
            entity.AfterInsert(
                "audit_post_insert",
                """RAISE NOTICE 'Post inserted: %', NEW."Id";""",
                ConstraintTriggerType.DeferrableInitiallyDeferred);
        });
    }
}