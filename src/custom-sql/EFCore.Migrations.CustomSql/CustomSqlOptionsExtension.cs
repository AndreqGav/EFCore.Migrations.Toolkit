using System.Collections.Generic;
using EFCore.Migrations.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Migrations.CustomSql
{
    public class CustomSqlOptionsExtension : IDbContextOptionsExtension
    {
        public CustomSqlOptionsExtension(DbContextOptionsBuilder optionsBuilder)
        {
            Info = new CustomSqlExtensionInfo(this);

            optionsBuilder.ReplaceService<IMigrationsModelDiffer, CompositeMigrationsModelDiffer>();
        }

        public void ApplyServices(IServiceCollection services)
        {
            services.AddSingleton<IMigrationOperationModifier, CustomSqlMigrationOperationModifier>();
        }

        public void Validate(IDbContextOptions options)
        {
        }

        public DbContextOptionsExtensionInfo Info { get; }
    }

    public class CustomSqlExtensionInfo : DbContextOptionsExtensionInfo
    {
        public CustomSqlExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

#if NET6_0_OR_GREATER
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is CustomSqlExtensionInfo;

        public override int GetServiceProviderHashCode() => 0;
#else
        public override long GetServiceProviderHashCode() => 0;
#endif

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["CustomSqlExtensionInfo"] = "1";
        }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "CustomSqlExtensionInfo";
    }
}