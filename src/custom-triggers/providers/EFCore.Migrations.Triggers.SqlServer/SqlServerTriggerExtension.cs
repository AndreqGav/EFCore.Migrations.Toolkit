using EFCore.Migrations.Triggers.Abstractions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Migrations.Triggers.SqlServer;

public class SqlServerTriggerExtension : TriggerSqlExtension
{
    public override void ApplyServices(IServiceCollection services)
    {
        base.ApplyServices(services);

        new EntityFrameworkServicesBuilder(services)
            .TryAddProviderSpecificServices(serviceMap =>
                serviceMap.TryAddSingleton<ITriggerSqlGenerator, SqlServerTriggerSqlGenerator>());
    }
}