using EFCore.Migrations.Triggers.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace EFCore.Migrations.Toolkit
{
    public static class SqlServerTriggersDependencyInjection
    {
        public static SqlServerDbContextOptionsBuilder UseTriggers(
            this SqlServerDbContextOptionsBuilder sqlServerBuilder)
        {
            var optionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)sqlServerBuilder).OptionsBuilder;
            optionsBuilder.UseTriggers<DbContextOptionsBuilder, SqlServerTriggerExtension>();

            return sqlServerBuilder;
        }
    }
}