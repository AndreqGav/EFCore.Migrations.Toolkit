using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models.Inheritance;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.AutoComments;

/// <summary>
/// Тесты проверяют, что конвенция автокомментариев корректно обрабатывает стратегии наследования TPH, TPC, TPT
/// </summary>
public class InheritanceStrategyConventionTests
{
    private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TContext>()
            .UseSqlite("Data Source=unit_tests.db")
            .UseAutoComments();

        return builder.Options;
    }

    private static string GetTableComment<TEntity>(DbContext context)
        => ModelAccessor.GetModel(context).FindEntityType(typeof(TEntity))!.GetComment();

    [Fact]
    public void AutoComments_Tph_Should_SetComment_OnBaseType()
    {
        // Arrange
        using var context = new TphAutoCommentsContext(BuildOptions<TphAutoCommentsContext>());

        // Act
        var comment = GetTableComment<PostBase>(context);

        // Assert
        Assert.Equal("Базовый тип в наследовании TPH.", comment);
    }

    [Fact]
    public void AutoComments_Tph_ShouldNot_SetComment_OnDerivedTypes()
    {
        // Arrange
        using var context = new TphAutoCommentsContext(BuildOptions<TphAutoCommentsContext>());

        // Act + Assert
        Assert.Null(GetTableComment<PostA>(context));
        Assert.Null(GetTableComment<PostB>(context));
    }

    [Fact]
    public void AutoComments_Tpc_ShouldNot_SetComment_OnAbstractBase()
    {
        // Arrange
        using var context = new TpcAutoCommentsContext(BuildOptions<TpcAutoCommentsContext>());

        // Act + Assert
        Assert.Null(GetTableComment<BlogBase>(context));
    }

    [Fact]
    public void AutoComments_Tpc_Should_SetComment_OnConcreteTypes()
    {
        // Arrange
        using var context = new TpcAutoCommentsContext(BuildOptions<TpcAutoCommentsContext>());

        // Act + Assert
        Assert.Equal("Наследник А в TPC.", GetTableComment<BlogA>(context));
        Assert.Equal("Наследник Б в TPC.", GetTableComment<BlogB>(context));
    }

    [Fact]
    public void AutoComments_Tpt_Should_SetComment_OnBaseType()
    {
        // Arrange
        using var context = new TptAutoCommentsContext(BuildOptions<TptAutoCommentsContext>());

        // Act + Assert
        Assert.Equal("Базовый тип в наследовании TPT.", GetTableComment<ArticleBase>(context));
    }

    [Fact]
    public void AutoComments_Tpt_Should_SetComment_OnDerivedTypes()
    {
        // Arrange
        using var context = new TptAutoCommentsContext(BuildOptions<TptAutoCommentsContext>());

        // Act + Assert
        Assert.Equal("Наследник А в TPT.", GetTableComment<ArticleA>(context));
        Assert.Equal("Наследник Б в TPT.", GetTableComment<ArticleB>(context));
    }
}

internal sealed class TphAutoCommentsContext : DbContext
{
    public DbSet<PostBase> Posts { get; set; }

    public DbSet<PostA> PostAs { get; set; }

    public DbSet<PostB> PostBs { get; set; }

    public TphAutoCommentsContext(DbContextOptions<TphAutoCommentsContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostBase>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.UseTphMappingStrategy();
        });

        modelBuilder.Entity<PostA>(b => b.HasBaseType<PostBase>());
        modelBuilder.Entity<PostB>(b => b.HasBaseType<PostBase>());
    }
}

internal sealed class TpcAutoCommentsContext : DbContext
{
    public DbSet<BlogA> BlogAs { get; set; }

    public DbSet<BlogB> BlogBs { get; set; }

    public TpcAutoCommentsContext(DbContextOptions<TpcAutoCommentsContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogBase>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.UseTpcMappingStrategy();
        });

        modelBuilder.Entity<BlogA>();
        modelBuilder.Entity<BlogB>();
    }
}

internal sealed class TptAutoCommentsContext : DbContext
{
    public DbSet<ArticleBase> Articles { get; set; }

    public DbSet<ArticleA> ArticleAs { get; set; }

    public DbSet<ArticleB> ArticleBs { get; set; }

    public TptAutoCommentsContext(DbContextOptions<TptAutoCommentsContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArticleBase>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.UseTptMappingStrategy();
        });

        modelBuilder.Entity<ArticleA>();
        modelBuilder.Entity<ArticleB>();
    }
}