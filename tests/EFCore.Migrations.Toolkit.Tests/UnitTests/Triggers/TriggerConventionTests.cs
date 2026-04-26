using System.Linq;
using EFCore.Migrations.CustomSql.Helpers;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models;
using EFCore.Migrations.Triggers;
using EFCore.Migrations.Triggers.Abstractions;
using EFCore.Migrations.Triggers.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.Triggers;

/// <summary>
/// Тесты проверяют, что конвенция триггеров правильно преобразует аннотации TriggerObject в SQL-аннотации CustomSql.
/// </summary>
public class TriggerConventionTests
{
    private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TContext>()
            .UseSqlite("Data Source=unit_tests.db")
            .UseTriggers<DbContextOptionsBuilder<TContext>, FakeTriggerProviderExtension>();

        return builder.Options;
    }

    private static string GetSingleSqlUp(DbContext context)
        => RelationalModelHelper.GetCustomSqlAnnotations(ModelAccessor.GetRelationalModel(context)).Single().SqlUp;

    private static string GetSingleSqlDown(DbContext context)
        => RelationalModelHelper.GetCustomSqlAnnotations(ModelAccessor.GetRelationalModel(context)).Single().SqlDown;

    [Fact]
    public void SingleTrigger_Should_ProduceOneAnnotation()
    {
        // Arrange
        using var context = new SingleTriggerContext(BuildOptions<SingleTriggerContext>());

        // Act
        var annotations = RelationalModelHelper
            .GetCustomSqlAnnotations(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Single(annotations);
    }

    [Fact]
    public void TriggerConvention_Should_StoreSqlUp_FromGenerator()
    {
        // Arrange
        using var context = new SingleTriggerContext(BuildOptions<SingleTriggerContext>());

        // Act
        var sqlUp = GetSingleSqlUp(context);

        // Assert
        Assert.NotNull(sqlUp);
        Assert.NotEmpty(sqlUp);
    }

    [Fact]
    public void TriggerConvention_Should_StoreSqlDown_FromGenerator()
    {
        // Arrange
        using var context = new SingleTriggerContext(BuildOptions<SingleTriggerContext>());

        // Act
        var sqlDown = GetSingleSqlDown(context);

        // Assert
        Assert.NotNull(sqlDown);
        Assert.NotEmpty(sqlDown);
    }

    [Fact]
    public void MultipleTriggers_Should_ProduceMultipleAnnotations()
    {
        // Arrange
        using var context = new TwoTriggersContext(BuildOptions<TwoTriggersContext>());

        // Act
        var annotations = RelationalModelHelper
            .GetCustomSqlAnnotations(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Equal(2, annotations.Count);
    }

    [Fact]
    public void MultipleTriggers_Should_HaveSeparateAnnotations_WithDifferentNames()
    {
        // Arrange
        using var context = new TwoTriggersContext(BuildOptions<TwoTriggersContext>());

        // Act
        var annotations = RelationalModelHelper
            .GetCustomSqlAnnotations(ModelAccessor.GetRelationalModel(context));

        // Assert
        Assert.Equal(2, annotations.Count);
        Assert.NotEqual(annotations[0].Name, annotations[1].Name);
    }

    [Fact]
    public void TriggerConvention_Should_StoreSqlUpAnnotation_OnEntityType()
    {
        // Arrange
        using var context = new SingleTriggerContext(BuildOptions<SingleTriggerContext>());

        // Act
        var annotation = RelationalModelHelper
            .GetCustomSqlAnnotations(ModelAccessor.GetRelationalModel(context))
            .Single();

        // Assert
        Assert.NotNull(annotation.SqlUp);
    }

    [Fact]
    public void TriggerConvention_Should_StoreSqlDownAnnotation_OnEntityType()
    {
        // Arrange
        using var context = new SingleTriggerContext(BuildOptions<SingleTriggerContext>());

        // Act
        var annotation = RelationalModelHelper
            .GetCustomSqlAnnotations(ModelAccessor.GetRelationalModel(context))
            .Single();

        // Assert
        Assert.NotNull(annotation.SqlDown);
    }
}

internal sealed class SingleTriggerContext : DbContext
{
    public const string TriggerName = "order_set_defaults";

    public const string TriggerBody = "PERFORM 1;";

    public DbSet<Order> Orders { get; set; }

    public SingleTriggerContext(DbContextOptions<SingleTriggerContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.AddTriggerAnnotation(new FakeTriggerObject
            {
                Name = TriggerName,
                Table = "Orders",
                Body = TriggerBody,
            });
        });
    }
}

internal sealed class TwoTriggersContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public TwoTriggersContext(DbContextOptions<TwoTriggersContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.AddTriggerAnnotation(new FakeTriggerObject
            {
                Name = "order_on_insert",
                Table = "Orders",
                Body = "body_a"
            });

            entity.AddTriggerAnnotation(new FakeTriggerObject
            {
                Name = "order_on_update",
                Table = "Orders",
                Body = "body_b"
            });
        });
    }
}

internal sealed class FakeTriggerProviderExtension : TriggerSqlExtension
{
    public override void ApplyServices(IServiceCollection services)
    {
        base.ApplyServices(services);

        new EntityFrameworkServicesBuilder(services)
            .TryAddProviderSpecificServices(serviceMap =>
                serviceMap.TryAddSingleton<ITriggerSqlGenerator, FakeTriggerSqlGenerator>());
    }
}

internal sealed class FakeTriggerSqlGenerator : ITriggerSqlGenerator
{
    public string GenerateCreateTriggerSql(TriggerObject trigger) => $"FAKE_CREATE";

    public string GenerateDeleteTriggerSql(TriggerObject trigger) => $"FAKE_DROP";
}

internal sealed record FakeTriggerObject : TriggerObject
{
}