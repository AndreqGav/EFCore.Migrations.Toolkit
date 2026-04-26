using System.Linq;
using EFCore.Migrations.Toolkit.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.MigrationTests.PostgreSQL;

[Collection("PostgreSQL Database tests")]
public class PostgreSqlMigrationTests
{
    [Fact]
    public void Migrations_Should_Apply_Successfully_And_Database_Be_Queryable()
    {
        // Arrange
        using var context = new PostgreSqlMigrationDbContext();
        context.Database.EnsureDeleted();

        // Act
        var exception = Record.Exception(() => context.Database.Migrate());

        // Assert
        Assert.Null(exception);
        Assert.Empty(context.Database.GetPendingMigrations());
        Assert.NotEmpty(context.Database.GetAppliedMigrations());
        Assert.True(context.Database.CanConnect(), "Не удалось подключиться к базе после миграций.");
        Assert.Equal(0, context.Blogs.Count());
        Assert.Equal(0, context.BlogViews.Count());
    }

    [Fact]
    public void Model_Should_Not_Have_Pending_Changes()
    {
        // Arrange
        using var context = new PostgreSqlMigrationDbContext();

        var differ = context.GetService<IMigrationsModelDiffer>();

        var sourceModel = GetSourceRelationalModel(context);
        var targetModel = ModelAccessor.GetRelationalModel(context);

        // Act
        var hasDifferences = differ.HasDifferences(sourceModel, targetModel);
        var differences = differ.GetDifferences(sourceModel, targetModel);

        var diffMessage = string.Empty;
        if (hasDifferences)
        {
            var diffs = differences.Select(d =>
            {
                // Если это SqlOperation, достаем сам SQL-код
                if (d is Microsoft.EntityFrameworkCore.Migrations.Operations.SqlOperation sqlOp)
                {
                    return $"SqlOperation: \n{sqlOp.Sql}\n(SuppressTransaction: {sqlOp.SuppressTransaction})";
                }

                return d.GetType().Name;
            });

            diffMessage = string.Join("\n\n", diffs);
        }

        // Assert
        Assert.False(hasDifferences,
            $"Обнаружены изменения ({differences.Count} шт.) в моделях DbContext, для которых не создана миграция.\nДетали:\n{diffMessage}\nВыполните 'dotnet ef migrations add'.");
    }

    private IRelationalModel GetSourceRelationalModel(PostgreSqlMigrationDbContext context)
    {
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();
        var snapshotModel = migrationsAssembly.ModelSnapshot?.Model;

        if (snapshotModel is null) return null;

        if (snapshotModel is IMutableModel mutableModel)
        {
            snapshotModel = mutableModel.FinalizeModel();
        }

        var modelRuntimeInitializer = context.GetService<IModelRuntimeInitializer>();
        snapshotModel = modelRuntimeInitializer.Initialize(snapshotModel);

        return snapshotModel.GetRelationalModel();
    }
}