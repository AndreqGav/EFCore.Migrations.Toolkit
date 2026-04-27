using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models.Schema;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.AutoComments;

/// <summary>
/// Тесты проверяют, что конвенция автокомментариев различает сущности с одинаковым именем таблицы в разных схемах.
/// </summary>
public class SchemaSeparationConventionTests
{
    private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
    {
        return new DbContextOptionsBuilder<TContext>()
            .UseSqlite("Data Source=unit_tests.db")
            .UseAutoComments()
            .Options;
    }

    private static string GetTableComment<TEntity>(DbContext context)
        => ModelAccessor.GetModel(context).FindEntityType(typeof(TEntity))!.GetComment();

    private static string GetColumnComment<TEntity>(DbContext context, string propertyName)
        => ModelAccessor.GetModel(context)
            .FindEntityType(typeof(TEntity))!
            .FindProperty(propertyName)!
            .GetComment();

    [Fact]
    public void AutoComments_SameTableName_DifferentSchemas_Should_SetIndependentTableComments()
    {
        // Arrange
        using var context = new DifferentSchemaContext(BuildOptions<DifferentSchemaContext>());

        // Act
        var domainComment = GetTableComment<DomainInvoice>(context);
        var billingComment = GetTableComment<BillingInvoice>(context);

        // Assert
        Assert.Equal("Счёт в домене.", domainComment);
        Assert.Equal("Счёт в биллинге.", billingComment);
    }

    [Fact]
    public void AutoComments_SameTableName_DifferentSchemas_Should_SetColumnComments()
    {
        // Arrange
        using var context = new DifferentSchemaContext(BuildOptions<DifferentSchemaContext>());

        // Act + Assert
        Assert.Equal("Номер счёта.", GetColumnComment<DomainInvoice>(context, nameof(DomainInvoice.Number)));
        Assert.Equal("Сумма счёта.", GetColumnComment<BillingInvoice>(context, nameof(BillingInvoice.Amount)));
    }

    [Fact]
    public void AutoComments_SameTableName_NullVsExplicitSchema_Should_SetIndependentComments()
    {
        // Arrange
        using var context = new NullAndExplicitSchemaContext(BuildOptions<NullAndExplicitSchemaContext>());

        // Act
        var domainComment = GetTableComment<DomainInvoice>(context);
        var billingComment = GetTableComment<BillingInvoice>(context);

        // Assert
        Assert.Equal("Счёт в домене.", domainComment);
        Assert.Equal("Счёт в биллинге.", billingComment);
    }
}

internal sealed class DifferentSchemaContext : DbContext
{
    public DifferentSchemaContext(DbContextOptions<DifferentSchemaContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DomainInvoice>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.ToTable("Invoices", "domain");
        });

        modelBuilder.Entity<BillingInvoice>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.ToTable("Invoices", "billing");
        });
    }
}

internal sealed class NullAndExplicitSchemaContext : DbContext
{
    public NullAndExplicitSchemaContext(DbContextOptions<NullAndExplicitSchemaContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // схема не задана (null)
        modelBuilder.Entity<DomainInvoice>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.ToTable("Invoices");
        });

        // явная схема
        modelBuilder.Entity<BillingInvoice>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.ToTable("Invoices", "billing");
        });
    }
}