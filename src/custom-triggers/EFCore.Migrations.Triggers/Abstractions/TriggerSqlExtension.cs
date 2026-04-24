using System.Collections.Generic;
using EFCore.Migrations.Triggers.Conventions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Migrations.Triggers.Abstractions
{
    public abstract class TriggerSqlExtension : IDbContextOptionsExtension
    {
        protected TriggerSqlExtension()
        {
            Info = new TriggerExtensionInfo(this);
        }

        public virtual void ApplyServices(IServiceCollection services)
        {
            new EntityFrameworkServicesBuilder(services)
                .TryAdd<IConventionSetPlugin, TriggerSqlSetPlugin>();
        }

        public virtual void Validate(IDbContextOptions options)
        {
        }

        public DbContextOptionsExtensionInfo Info { get; }
    }

    public class TriggerExtensionInfo : DbContextOptionsExtensionInfo
    {
        public TriggerExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

#if NET6_0_OR_GREATER
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is TriggerExtensionInfo;

        public override int GetServiceProviderHashCode() => 0;
#else
        public override long GetServiceProviderHashCode() => 0;
#endif

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["TriggerExtension"] = "1";
        }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "TriggerExtension";
    }
}