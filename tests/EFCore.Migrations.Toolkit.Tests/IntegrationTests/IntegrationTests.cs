using System;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.IntegrationTests;

/// <summary>
/// Интеграционные тесты.
/// </summary>
public class IntegrationTests
{
    private static DbContextOptions<IntegrationDbContext> BuildNpgsqlOptions()
    {
        var builder = new DbContextOptionsBuilder<IntegrationDbContext>()
            .UseSqlite("Data Source=unit_tests.db")
            .UseCustomSql()
            .UseAutoComments(options => options.AddEnumDescriptions().FromXmlFiles("EFCore.Migrations.Toolkit.Tests.xml"));

        return builder.Options;
    }

    [Fact]
    public void NonSqlDatabase_Should_Ignore()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder<IntegrationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

        // Act & Assert
        using var context = new IntegrationDbContext(optionsBuilder.Options);
        context.Database.EnsureCreated();
    }

    [Fact]
    public void Extensions_Should_Not_Throw_ManyServiceProvidersCreatedWarning()
    {
        for (var i = 0; i < 100; i++)
        {
            var options = BuildNpgsqlOptions();

            using var context = new IntegrationDbContext(options);

            _ = context.Model;
        }
    }
}

public class IntegrationDbContext : DbContext
{
    public IntegrationDbContext()
    {
    }

    public IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : base(options)
    {
    }
}