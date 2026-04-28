using System;
using EFCore.Migrations.CustomSql;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models;
using EFCore.Migrations.Triggers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.IntegrationTests.PostgreSQL;

/// <summary>
/// Интеграционные тесты PostgreSQL.
/// </summary>
[Collection("PostgreSQL Database tests")]
public class PostgreSqlIntegrationTests : IDisposable
{
    private readonly PostgreSqlTestDbContext _context;

    public PostgreSqlIntegrationTests()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlTestDbContext>()
            .UseNpgsql(PostgreSqlDatabase.ConnectionString)
            .UseAutoComments()
            .UseCustomSql(o => o.UseNpgsql());

        _context = new PostgreSqlTestDbContext(optionsBuilder.Options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void Migration_Script_Should_Contain_CreateOrReplaceFunction()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE OR REPLACE FUNCTION get_blog_name", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_CreateView()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE VIEW blog_view", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_CommentOn()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("COMMENT ON", script);
    }

    [Fact]
    public void Migration_Script_Should_Contain_CreateTrigger()
    {
        var script = _context.Database.GenerateCreateScript();

        Assert.Contains("CREATE FUNCTION", script);
        Assert.Contains("CREATE TRIGGER", script);
    }

    [Fact]
    public void Function_Should_Exist_InDatabase()
    {
        var count = ExecuteScalar<long>("SELECT COUNT(*) FROM pg_proc WHERE proname = 'get_blog_name'");

        Assert.Equal(1, count);
    }

    [Fact]
    public void View_Should_Exist_InDatabase()
    {
        var count = ExecuteScalar<long>("SELECT COUNT(*) FROM pg_views WHERE viewname = 'blog_view'");

        Assert.Equal(1, count);
    }

    [Fact]
    public void Trigger_Should_Exist_InDatabase()
    {
        var count = ExecuteScalar<long>("SELECT COUNT(*) FROM pg_trigger WHERE tgname = 'trg_order_set_defaults'");

        Assert.Equal(1, count);
    }

    [Fact]
    public void BeforeInsert_Trigger_Should_Fire_OnInsert()
    {
        // Триггер trg_order_set_defaults устанавливает IsConfirmed = false перед вставкой
        var order = new Order
        {
            TotalAmount = 100m,
            IsConfirmed = true
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        _context.ChangeTracker.Clear();

        var saved = _context.Orders.Find(order.Id);
        Assert.False(saved.IsConfirmed);
    }

    [Fact]
    public void Multiple_Triggers_Should_Exist_InDatabase()
    {
        var count = ExecuteScalar<long>(
            "SELECT COUNT(*) FROM pg_trigger WHERE tgname IN ('trg_order_set_defaults', 'trg_order_prevent_negative_amount')");

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

internal class PostgreSqlTestDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public DbSet<Blog> Blogs { get; set; }

    public PostgreSqlTestDbContext(DbContextOptions<PostgreSqlTestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddCustomSql(
            "blog_view",
            "CREATE VIEW blog_view AS SELECT * FROM \"Blogs\"",
            "DROP VIEW IF EXISTS blog_view"
        );

        modelBuilder.AddCustomSql("get_blog_name",
            "CREATE OR REPLACE FUNCTION get_blog_name(id integer)\nRETURNS text AS $$\nBEGIN\nRETURN (SELECT \"Name\" FROM \"Blogs\" WHERE \"Id\" = id);\n END;\n$$ LANGUAGE plpgsql;",
            "DROP FUNCTION IF EXISTS get_blog_name"
        );

        modelBuilder.Entity<Order>(entity =>
        {
            entity.BeforeInsert(
                "trg_order_set_defaults",
                "NEW.\"IsConfirmed\" := false;");

            entity.BeforeInsert(
                "trg_order_prevent_negative_amount",
                "IF NEW.\"TotalAmount\" < 0 THEN RAISE EXCEPTION 'amount must not be negative'; END IF;");
        });

        modelBuilder.Entity<BlogView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("blog_view");
        });
    }
}