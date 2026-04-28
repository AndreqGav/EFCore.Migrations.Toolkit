using EFCore.Migrations.CustomSql.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.Toolkit;

public static class DependencyInjection
{
    public static CustomSqlOptionsBuilder UseNpgsql(this CustomSqlOptionsBuilder customSqlOptionsBuilder)
    {
        var optionsBuilder = ((ICustomSqlOptionsBuilder)customSqlOptionsBuilder).OptionsBuilder;

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new PostgreSqlProviderExtension());

        return customSqlOptionsBuilder;
    }
}