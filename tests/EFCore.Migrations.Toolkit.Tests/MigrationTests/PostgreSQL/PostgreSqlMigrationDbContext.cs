using EFCore.Migrations.CustomSql;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.MigrationTests.PostgreSQL.Sql;
using EFCore.Migrations.Toolkit.Tests.Models;
using EFCore.Migrations.Toolkit.Tests.Models.Inheritance;
using EFCore.Migrations.Triggers;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Migrations.Toolkit.Tests.MigrationTests.PostgreSQL
{
    public class PostgreSqlMigrationDbContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }

        public DbSet<BlogView> BlogViews { get; set; }

        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(PostgreSqlDatabase.ConnectionString, o => o.UseTriggers())
                .UseCustomSql()
                .UseAutoComments(options => options.AddEnumDescriptions());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureViews(modelBuilder);
            ConfigureBlogEntities(modelBuilder);
            ConfigureOrderTriggers(modelBuilder);
            ConfigureDbFunctions(modelBuilder);
            ConfigureTphInheritance(modelBuilder);
            ConfigureTptInheritance(modelBuilder);
            ConfigureTpcInheritance(modelBuilder);
        }

        private void ConfigureViews(ModelBuilder modelBuilder)
        {
            // Представление blog_names строится через генератор, использующий метаданные модели
            var viewSqlGenerator = new BlogViewSqlGenerator(this, modelBuilder);
            modelBuilder.AddCustomSql("blog_names", viewSqlGenerator.Create(), viewSqlGenerator.Drop());

            modelBuilder.Entity<BlogView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("blog_view");

                // Ручной SQL для представления blogs_view
                entity.AddCustomSql(
                    "blog_view",
                    "CREATE VIEW blog_view AS SELECT * FROM \"Blogs\"",
                    "DROP VIEW IF EXISTS blog_view");

            });
        }

        private void ConfigureBlogEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                var triggerGenerator = new BlogTriggerSqlGenerator(this, modelBuilder);

                entity.BeforeInsertOrUpdate(
                    "before_insert_or_update_blog",
                    triggerGenerator.GenerateTriggerBody());
            });
        }

        private static void ConfigureOrderTriggers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.BeforeInsert("set_order_defaults", "NEW.is_confirmed = false;");

                entity.BeforeUpdate(
                    "prevent_update_negative_amount",
                    "IF NEW.total_amount < 0 THEN RAISE EXCEPTION 'amount negative'; END IF;");
            });
        }

        private static void ConfigureDbFunctions(ModelBuilder modelBuilder)
        {
            modelBuilder
                .AddCustomSql("get_blog_name", BlogFunctionSql.Up(), BlogFunctionSql.Down())
                .HasDbFunction(typeof(BlogFunctionSql).GetMethod(nameof(BlogFunctionSql.GetName))!)
                .HasName("get_blog_name");
        }

        private static void ConfigureTphInheritance(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostBase>(builder =>
            {
                builder.HasKey(entity => entity.Id);
            });

            modelBuilder.Entity<PostA>(builder => builder.HasBaseType<PostBase>());
            modelBuilder.Entity<PostB>(builder => builder.HasBaseType<PostBase>());
        }

        private static void ConfigureTptInheritance(ModelBuilder modelBuilder)
        {
        }

        private static void ConfigureTpcInheritance(ModelBuilder modelBuilder)
        {
        }
    }
}