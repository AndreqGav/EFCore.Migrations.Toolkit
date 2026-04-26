using System;
using System.Linq;
using EFCore.Migrations.CustomSql;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using EFCore.Migrations.Toolkit.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.CustomSql;

/// <summary>
/// Тесты проверяют генерацию операций миграции через <see cref="CustomSqlMigrationOperationModifier"/>.
/// </summary>
public class CustomSqlMigrationOperationModifierTests
{
    internal const string SqlUp =
        "CREATE VIEW orders_summary AS SELECT id, number, total_amount FROM \"Orders\";";

    internal const string SqlDown = "DROP VIEW IF EXISTS orders_summary;";

    // Изменённая версия того же SQL для проверки сценариев смены скрипта
    internal const string ChangedSqlUp =
        "CREATE VIEW orders_summary AS SELECT id, number, total_amount, 'v2' AS version FROM \"Orders\";";

    private static DbContextOptions<TContext> BuildOptions<TContext>() where TContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TContext>();
        builder.UseSqlite("Data Source=unit_tests.db").UseCustomSql();

        return builder.Options;
    }

    [Fact]
    public void ModifyOperations_Should_ProduceCreateOperation_WhenAnnotationIsNew()
    {
        // Arrange
        using var emptyContext = new EmptyCustomSqlContext(BuildOptions<EmptyCustomSqlContext>());
        using var sqlContext = new CustomSqlContext(BuildOptions<CustomSqlContext>());

        // Act
        var modifier = new CustomSqlMigrationOperationModifier();
        var operations = modifier.ModifyOperations(
            Array.Empty<MigrationOperation>(),
            ModelAccessor.GetRelationalModel(emptyContext),
            ModelAccessor.GetRelationalModel(sqlContext));

        // Assert
        Assert.Contains(operations, o => o is SqlOperation s && s.Sql == SqlUp);
    }

    [Fact]
    public void ModifyOperations_Should_ProduceDeleteOperation_WhenAnnotationRemoved()
    {
        // Arrange
        using var sqlContext = new CustomSqlContext(BuildOptions<CustomSqlContext>());
        using var emptyContext = new EmptyCustomSqlContext(BuildOptions<EmptyCustomSqlContext>());

        // Act
        var modifier = new CustomSqlMigrationOperationModifier();
        var operations = modifier.ModifyOperations(
            Array.Empty<MigrationOperation>(),
            ModelAccessor.GetRelationalModel(sqlContext),
            ModelAccessor.GetRelationalModel(emptyContext));

        // Assert
        Assert.Contains(operations, o => o is SqlOperation s && s.Sql == SqlDown);
    }

    [Fact]
    public void ModifyOperations_Should_ProduceNoOperations_WhenAnnotationUnchanged()
    {
        // Arrange
        using var sourceContext = new CustomSqlContext(BuildOptions<CustomSqlContext>());
        using var targetContext = new CustomSqlContext(BuildOptions<CustomSqlContext>());

        // Act
        var modifier = new CustomSqlMigrationOperationModifier();
        var operations = modifier.ModifyOperations(
            Array.Empty<MigrationOperation>(),
            ModelAccessor.GetRelationalModel(sourceContext),
            ModelAccessor.GetRelationalModel(targetContext));

        // Assert
        Assert.Empty(operations);
    }

    [Fact]
    public void ModifyOperations_Should_ProduceDeleteThenCreate_WhenSqlUpChanged()
    {
        // Arrange
        using var originalContext = new CustomSqlContext(BuildOptions<CustomSqlContext>());
        using var changedContext = new ChangedSqlContext(BuildOptions<ChangedSqlContext>());

        // Act
        var modifier = new CustomSqlMigrationOperationModifier();
        var operations = modifier.ModifyOperations(
                Array.Empty<MigrationOperation>(),
                ModelAccessor.GetRelationalModel(originalContext),
                ModelAccessor.GetRelationalModel(changedContext))
            .OfType<SqlOperation>()
            .ToList();

        // Assert
        Assert.Contains(operations, o => o.Sql == SqlDown);
        Assert.Contains(operations, o => o.Sql == ChangedSqlUp);
        Assert.DoesNotContain(operations, o => o.Sql == SqlUp);

        var deleteIdx = operations.FindIndex(o => o.Sql == SqlDown);
        var createIdx = operations.FindIndex(o => o.Sql == ChangedSqlUp);
        Assert.True(deleteIdx < createIdx, "Удаление должно идти до создания");
    }

    [Fact]
    public void ModifyOperations_Should_PlacePassthroughOps_BeforeCreateOperations()
    {
        // Arrange — passthrough-операция (например, ALTER TABLE) идёт между удалением и созданием
        using var emptyContext = new EmptyCustomSqlContext(BuildOptions<EmptyCustomSqlContext>());
        using var sqlContext = new CustomSqlContext(BuildOptions<CustomSqlContext>());

        var passthroughOp = new SqlOperation
        {
            Sql = "SELECT 1;"
        };

        // Act
        var modifier = new CustomSqlMigrationOperationModifier();
        var operations = modifier.ModifyOperations(
                new[]
                {
                    passthroughOp
                },
                ModelAccessor.GetRelationalModel(emptyContext),
                ModelAccessor.GetRelationalModel(sqlContext))
            .ToList();

        // Assert — passthrough-операции идут до CREATE-операций
        var passthroughIdx = operations.IndexOf(passthroughOp);
        var createIdx = operations.FindIndex(o => o is SqlOperation s && s.Sql == SqlUp);
        Assert.True(passthroughIdx >= 0);
        Assert.True(passthroughIdx < createIdx, "Passthrough-операции должны идти до CREATE-операций");
    }

    internal sealed class EmptyCustomSqlContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public EmptyCustomSqlContext(DbContextOptions<EmptyCustomSqlContext> options) : base(options)
        {
        }
    }

    internal sealed class CustomSqlContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public CustomSqlContext(DbContextOptions<CustomSqlContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddCustomSql(
                CustomSqlAnnotationTests.SqlName,
                CustomSqlAnnotationTests.SqlUp,
                CustomSqlAnnotationTests.SqlDown);
        }
    }

    internal sealed class ChangedSqlContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public ChangedSqlContext(DbContextOptions<ChangedSqlContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddCustomSql(
                CustomSqlAnnotationTests.SqlName,
                CustomSqlAnnotationTests.ChangedSqlUp,
                CustomSqlAnnotationTests.SqlDown);
        }
    }
}