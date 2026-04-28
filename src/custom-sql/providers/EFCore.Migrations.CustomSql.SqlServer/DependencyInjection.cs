using EFCore.Migrations.CustomSql.SqlServer;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.Toolkit;

public static class SqlServerTriggersDependencyInjection
{
    public static CustomSqlOptionsBuilder UseSqlServer(this CustomSqlOptionsBuilder customSqlOptionsBuilder)
    {
        var optionsBuilder = ((ICustomSqlOptionsBuilder)customSqlOptionsBuilder).OptionsBuilder;

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new SqlServerProviderExtension());

        return customSqlOptionsBuilder;
    }
}