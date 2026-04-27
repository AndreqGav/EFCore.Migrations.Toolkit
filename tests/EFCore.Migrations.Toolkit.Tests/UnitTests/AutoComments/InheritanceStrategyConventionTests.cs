using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models.Inheritance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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

    private static DbContextOptions<TContext> BuildOptionsWithCombineInheritance<TContext>() where TContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TContext>()
            .UseSqlite("Data Source=unit_tests.db")
            .UseAutoComments(o => o.FromXmlFiles().AddInheritanceComments());

        return builder.Options;
    }

    private static string GetTableComment<TEntity>(DbContext context)
        => ModelAccessor.GetModel(context).FindEntityType(typeof(TEntity))!.GetComment();

    private static IProperty GetProperty<TEntity>(DbContext context, string propertyName)
        => ModelAccessor.GetModel(context)
            .FindEntityType(typeof(TEntity))!
            .FindProperty(propertyName)!;

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
    public void AutoComments_Tph_CombineInheritance_Should_MergeAllComments_OnBaseType()
    {
        // Arrange
        using var context = new TphAutoCommentsContext(BuildOptionsWithCombineInheritance<TphAutoCommentsContext>());

        // Act
        var comment = GetTableComment<PostBase>(context);

        // Assert
        Assert.Contains("Базовый тип в наследовании TPH.", comment);
        Assert.Contains("Наследник А.", comment);
        Assert.Contains("Наследник Б.", comment);
    }

    [Fact]
    public void AutoComments_Tph_CombineInheritance_Should_SetMergedComment_OnDerivedType()
    {
        // Arrange
        using var context = new TphAutoCommentsContext(BuildOptionsWithCombineInheritance<TphAutoCommentsContext>());

        // Act + Assert
        Assert.Equal(GetTableComment<PostA>(context), GetTableComment<PostB>(context));

        Assert.Contains("Базовый тип в наследовании TPH.", GetTableComment<PostA>(context));
        Assert.Contains("Наследник А.", GetTableComment<PostA>(context));
        Assert.Contains("Наследник Б.", GetTableComment<PostA>(context));
    }

    [Fact]
    public void AutoComments_Tph_ShouldNot_SetSameComment_OnDerivedTypes()
    {
        // Arrange
        using var context = new TphAutoCommentsContext(BuildOptions<TphAutoCommentsContext>());

        // Act + Assert
        Assert.Equal(GetTableComment<PostBase>(context), GetTableComment<PostA>(context));
        Assert.Equal(GetTableComment<PostBase>(context), GetTableComment<PostB>(context));
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
        });

        modelBuilder.Entity<PostA>(b => b.HasBaseType<PostBase>());
        modelBuilder.Entity<PostB>(b => b.HasBaseType<PostBase>());
    }
}

internal sealed class TphSameColumnSameCommentContext : DbContext
{
    public TphSameColumnSameCommentContext(DbContextOptions<TphSameColumnSameCommentContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationBase>(builder =>
        {
            builder.HasKey(e => e.Id);
        });

        modelBuilder.Entity<SmsNotification>(builder =>
        {
            builder.HasBaseType<NotificationBase>();

            builder.Property(e => e.Content).HasColumnName("Content");
        });

        modelBuilder.Entity<EmailNotification>(builder =>
        {
            builder.HasBaseType<NotificationBase>();

            builder.Property(e => e.Content).HasColumnName("Content");
        });
    }
}

internal sealed class TphSameColumnDiffCommentContext : DbContext
{
    public TphSameColumnDiffCommentContext(DbContextOptions<TphSameColumnDiffCommentContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationBase>(b =>
        {
            b.HasKey(e => e.Id);
        });

        modelBuilder.Entity<SmsNotification>(builder =>
        {
            builder.HasBaseType<NotificationBase>();

            builder.Property(e => e.Content).HasColumnName("Content");
        });

        modelBuilder.Entity<SystemNotification>(builder =>
        {
            builder.HasBaseType<NotificationBase>();

            builder.Property(e => e.Content).HasColumnName("Content");
        });
    }
}