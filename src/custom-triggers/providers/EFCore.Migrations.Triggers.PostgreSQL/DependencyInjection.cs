using EFCore.Migrations.Triggers.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.Toolkit;

public static class DependencyInjection
{
    public static NpgsqlDbContextOptionsBuilder UseTriggers(this NpgsqlDbContextOptionsBuilder npgsqlBuilder)
    {
        var optionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)npgsqlBuilder).OptionsBuilder;
        optionsBuilder.UseTriggers<DbContextOptionsBuilder, PostgresqlTriggerExtension>();

        return npgsqlBuilder;
    }
}