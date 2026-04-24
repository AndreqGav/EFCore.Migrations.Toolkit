using EFCore.Migrations.CustomSql;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models;
using EFCore.Migrations.Toolkit.Tests.Models.Inheritance;
using EFCore.Migrations.Triggers;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Migrations.Toolkit.Tests.MigrationTests.SqlServer
{
    public class SqlServerMigrationDbContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }

        public DbSet<BlogView> BlogViews { get; set; }

        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(SqlServerTestDatabase.ConnectionString, o => o.UseTriggers())
                .UseCustomSql();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureViews(modelBuilder);
            ConfigureBlogEntities(modelBuilder);
            ConfigureOrderTriggers(modelBuilder);
            ConfigureDbObjects(modelBuilder);
            ConfigureTphInheritance(modelBuilder);
        }

        private static void ConfigureViews(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BlogView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("blog_view");

                entity.AddCustomSql(
                    "blog_view",
                    "CREATE VIEW blog_view AS SELECT * FROM [Blogs]",
                    "DROP VIEW IF EXISTS blog_view");
            });
        }

        private static void ConfigureBlogEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.AfterInsertOrUpdate(
                    "trg_blog_log_changes",
                    "-- log blog insert or update");
            });
        }

        private static void ConfigureOrderTriggers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.AfterInsert("trg_order_set_confirmed",
                    "UPDATE [Orders] SET [IsConfirmed] = 0 WHERE [Id] IN (SELECT [Id] FROM inserted)");

                entity.AfterUpdate("trg_order_prevent_negative_amount",
                    "IF EXISTS (SELECT 1 FROM inserted WHERE [TotalAmount] < 0)\r\n    THROW 50001, 'Amount must not be negative', 1;");
            });
        }

        private static void ConfigureDbObjects(ModelBuilder modelBuilder)
        {
            modelBuilder.AddCustomSql(
                "get_blog_name",
                "CREATE OR ALTER PROCEDURE [get_blog_name] @id INT AS SELECT [Name] FROM [Blogs] WHERE [Id] = @id",
                "DROP PROCEDURE IF EXISTS [get_blog_name]");
        }

        private static void ConfigureTphInheritance(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostBase>(builder =>
            {
                builder.HasKey(entity => entity.Id);

#if NET7_0_OR_GREATER
                builder.UseTphMappingStrategy();
#endif
            });

            modelBuilder.Entity<PostA>(builder => builder.HasBaseType<PostBase>());
            modelBuilder.Entity<PostB>(builder => builder.HasBaseType<PostBase>());
        }
    }
}