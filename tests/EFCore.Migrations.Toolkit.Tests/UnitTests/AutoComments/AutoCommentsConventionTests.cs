using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.AutoComments;

/// <summary>
/// Тесты проверяют, что конвенция автокомментариев корректно устанавливает комментарии на таблицы и колонки на основе XML-документации.
/// </summary>
public class AutoCommentsConventionTests
{
    private static DbContextOptions<AutoCommentsContext> BuildOptions(bool globalEnumDescriptions = false)
    {
        var builder = new DbContextOptionsBuilder<AutoCommentsContext>()
            .UseSqlite("Data Source=unit_tests.db")
            .UseAutoComments(options =>
            {
                if (globalEnumDescriptions)
                {
                    options.AddEnumDescriptions();
                }
            });

        return builder.Options;
    }

    private static string GetTableComment<TEntity>(DbContext context)
        => ModelAccessor.GetModel(context).FindEntityType(typeof(TEntity))!.GetComment();

    private static string GetColumnComment<TEntity>(DbContext context, string propertyName)
        => ModelAccessor.GetModel(context)
            .FindEntityType(typeof(TEntity))!
            .FindProperty(propertyName)!
            .GetComment();

    [Fact]
    public void AutoComments_Should_SetTableComment()
    {
        // Arrange
        using var context = new AutoCommentsContext(BuildOptions());

        // Act
        var comment = GetTableComment<Order>(context);

        // Assert
        Assert.Equal("Заказ покупателя.", comment);
    }

    [Fact]
    public void AutoComments_Should_SetColumnComment()
    {
        // Arrange
        using var context = new AutoCommentsContext(BuildOptions());

        // Act
        var idComment = GetColumnComment<Order>(context, nameof(Order.Id));
        var numberComment = GetColumnComment<Order>(context, nameof(Order.Number));

        // Assert
        Assert.Equal("Идентификатор заказа.", idComment);
        Assert.Equal("Номер заказа.", numberComment);
    }

    [Fact]
    public void AutoComments_Should_SetTrimmedColumnComment()
    {
        // Arrange
        using var context = new AutoCommentsContext(BuildOptions());

        // Act
        var comment = GetColumnComment<Order>(context, nameof(Order.TotalAmount));

        // Assert
        Assert.Equal("Итоговая сумма заказа в рублях.", comment);
    }

    [Fact]
    public void AutoComments_Should_AppendEnumValues_ToIntColumn_WhenHasEnumDescriptionsAttribute()
    {
        // Arrange
        using var context = new AutoCommentsContext(BuildOptions());

        // Act
        var comment = GetColumnComment<Order>(context, nameof(Order.Status));

        // Assert
        Assert.StartsWith("Статус заказа.\n", comment);
        Assert.Contains("0 - Активный, ожидает выполнения.", comment);
        Assert.Contains("1 - Выполнен, доставлен покупателю.", comment);
        Assert.Contains("2 - Отменён, возврат средств.", comment);
    }

    [Fact]
    public void AutoComments_ShouldNot_AppendEnumValues_WhenHasNoEnumDescriptionsAttribute()
    {
        // Arrange
        using var context = new AutoCommentsContext(BuildOptions());

        // Act
        var comment = GetColumnComment<Order>(context, nameof(Order.Category));

        // Assert
        Assert.Equal("Категория заказа.", comment);
    }

    [Fact]
    public void AutoComments_Should_AppendEnumValues_ToStringColumn_WhenGlobalEnableEnumDescriptions()
    {
        // Arrange
        using var context = new AutoCommentsContext(BuildOptions(globalEnumDescriptions: true));

        // Act
        var comment = GetColumnComment<Order>(context, nameof(Order.Category));

        // Assert
        Assert.StartsWith("Категория заказа.\n", comment);
        Assert.Contains("Clothing - Одежда.", comment);
        Assert.Contains("Books - Книги.", comment);
        Assert.Contains("Toys - Игрушки.", comment);
    }

    [Fact]
    public void AutoComments_ShouldNot_AppendEnumValues_WhenIgnoreAutoCommentEnumDescription()
    {
        // Arrange
        using var context = new AutoCommentsContext(BuildOptions(globalEnumDescriptions: true));

        // Act
        var comment = GetColumnComment<Order>(context, nameof(Order.DeliveryMethod));

        // Assert
        Assert.Equal("Способ доставки.", comment);
    }

    [Fact]
    public void AutoComments_Should_NotOverwrite_ManualTableComment()
    {
        // Arrange
        using var context = new AutoCommentsContext(BuildOptions());

        // Act
        var comment = GetTableComment<Blog>(context);

        // Assert
        Assert.Equal("Блог (ручной комментарий)", comment);
    }

    [Fact]
    public void AutoComments_Should_NotOverwrite_ManualPropertyComment()
    {
        // Arrange
        using var context = new AutoCommentsContext(BuildOptions());

        // Act
        var comment = GetColumnComment<Blog>(context, nameof(Blog.Url));

        // Assert
        Assert.Equal("URL (ручной комментарий)", comment);
    }

    [Fact]
    public void AutoComments_Should_SkipView_AndNotSetComment()
    {
        // Arrange
        using var context = new AutoCommentsContext(BuildOptions());

        // Act
        var comment = GetTableComment<OrderCatalogView>(context);

        // Assert
        Assert.Null(comment);
    }

    [Fact]
    public void AutoComments_Should_SkipSqlQuery_AndNotSetComment()
    {
        // Arrange
        using var context = new AutoCommentsContext(BuildOptions());

        // Act
        var comment = GetTableComment<OrderCatalogSqlQuery>(context);

        // Assert
        Assert.Null(comment);
    }
}

internal sealed class AutoCommentsContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public DbSet<Blog> Blogs { get; set; }

    public DbSet<OrderCatalogView> OrderCatalogViews { get; set; }

    public AutoCommentsContext(DbContextOptions<AutoCommentsContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Представление - конвенция автокомментариев должна его пропускать
        modelBuilder.Entity<OrderCatalogView>(builder =>
        {
            builder.HasNoKey();
            builder.ToView("OrderCatalog");
        });

        modelBuilder.Entity<OrderCatalogSqlQuery>(builder =>
        {
            builder.HasNoKey();
            builder.ToSqlQuery("SELECT * FROM \"Orders\"");
        });

        modelBuilder.Entity<Order>(builder => builder.Property(e => e.Category).HasConversion<string>());

        // Ручной комментарий - конвенция не должна его перезаписывать
        modelBuilder.Entity<Blog>(builder =>
        {
            builder.HasComment("Блог (ручной комментарий)");
            builder.ToTable("Blogs");
            builder.Property(b => b.Url).HasComment("URL (ручной комментарий)");
        });
    }
}