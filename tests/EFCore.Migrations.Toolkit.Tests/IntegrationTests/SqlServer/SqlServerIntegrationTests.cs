using System;
using EFCore.Migrations.CustomSql;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models;
using EFCore.Migrations.Triggers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.IntegrationTests.SqlServer
{
    [Collection("SqlServer Database tests")]
    public class SqlServerIntegrationTests : IDisposable
    {
        private readonly SqlServerTestDbContext _context;

        public SqlServerIntegrationTests()
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqlServerTestDbContext>()
                .UseSqlServer(SqlServerTestDatabase.ConnectionString, o => o.UseTriggers())
                .UseCustomSql();

            _context = new SqlServerTestDbContext(optionsBuilder.Options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void Migration_Script_Should_Contain_CreateView()
        {
            // Arrange
            var script = _context.Database.GenerateCreateScript();

            // Act & Assert
            Assert.Contains("CREATE VIEW blog_view", script);
        }

        [Fact]
        public void Migration_Script_Should_Contain_CreateOrAlterProcedure()
        {
            // Arrange
            var script = _context.Database.GenerateCreateScript();

            // Act & Assert
            Assert.Contains("CREATE OR ALTER PROCEDURE", script);
        }

        [Fact]
        public void Migration_Script_Should_Contain_CreateOrAlterTrigger()
        {
            // Arrange
            var script = _context.Database.GenerateCreateScript();

            // Act & Assert
            Assert.Contains("CREATE OR ALTER TRIGGER", script);
        }

        [Fact]
        public void View_Should_Exist_InDatabase()
        {
            // Arrange
            var count = ExecuteScalar<int>(
                "SELECT COUNT(*) FROM sys.objects WHERE name = 'blog_view' AND type = 'V'");

            // Act & Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public void Procedure_Should_Exist_InDatabase()
        {
            // Arrange
            var count = ExecuteScalar<int>(
                "SELECT COUNT(*) FROM sys.objects WHERE name = 'get_blog_name' AND type = 'P'");

            // Act & Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public void Trigger_Should_Exist_InDatabase()
        {
            // Arrange
            var count = ExecuteScalar<int>(
                "SELECT COUNT(*) FROM sys.triggers WHERE name = 'trg_order_set_confirmed'");

            // Act & Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public void AfterInsert_Trigger_Should_Fire_OnInsert()
        {
            // Arrange
            var order = new Order
            {
                TotalAmount = 100m,
                IsConfirmed = true,
            };

            // Act
            _context.Orders.Add(order);
            _context.SaveChanges();

            _context.ChangeTracker.Clear();

            // Assert
            var saved = _context.Orders.Find(order.Id);
            Assert.False(saved.IsConfirmed);
        }

        [Fact]
        public void Multiple_Triggers_Should_Exist_InDatabase()
        {
            // Arrange
            var count = ExecuteScalar<int>(
                "SELECT COUNT(*) FROM sys.triggers WHERE name IN ('trg_order_set_confirmed', 'trg_order_prevent_negative_amount')");

            // Act & Assert
            Assert.Equal(2, count);
        }

        private T ExecuteScalar<T>(string sql)
        {
            var conn = _context.Database.GetDbConnection();
            var wasOpen = conn.State == System.Data.ConnectionState.Open;
            if (!wasOpen) conn.Open();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;

                return (T)cmd.ExecuteScalar();
            }
            finally
            {
                if (!wasOpen) conn.Close();
            }
        }
    }

    internal class SqlServerTestDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public DbSet<Blog> Blogs { get; set; }

        public SqlServerTestDbContext(DbContextOptions<SqlServerTestDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddCustomSql(
                "blog_view",
                "CREATE VIEW blog_view AS SELECT * FROM [Blogs]",
                "DROP VIEW IF EXISTS blog_view");

            modelBuilder.AddCustomSql(
                "get_blog_name",
                "CREATE OR ALTER PROCEDURE [get_blog_name] @id INT AS SELECT [Name] FROM [Blogs] WHERE [Id] = @id",
                "DROP PROCEDURE IF EXISTS [get_blog_name]");

            modelBuilder.Entity<Order>(entity =>
            {
                // Sets IsConfirmed = false after every insert
                entity.AfterInsert(
                    "trg_order_set_confirmed",
                    "UPDATE [Orders] SET [IsConfirmed] = 0 WHERE [Id] IN (SELECT [Id] FROM inserted)");

                entity.AfterInsert(
                    "trg_order_prevent_negative_amount",
                    "IF EXISTS (SELECT 1 FROM inserted WHERE [TotalAmount] < 0)\r\n    THROW 50001, 'Amount must not be negative', 1;");
            });

            modelBuilder.Entity<BlogView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("blog_view");
            });
        }
    }
}