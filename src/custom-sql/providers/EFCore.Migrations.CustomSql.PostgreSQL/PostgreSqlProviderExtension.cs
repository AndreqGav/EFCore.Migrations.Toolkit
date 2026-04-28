using EFCore.Migrations.CustomSql.PostgreSQL.Triggers;
using EFCore.Migrations.Triggers.Abstractions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.Migrations.CustomSql.PostgreSQL;

public class PostgreSqlProviderExtension : TriggerSqlExtension
{
    public override void ApplyServices(IServiceCollection services)
    {
        base.ApplyServices(services);

        new EntityFrameworkServicesBuilder(services)
            .TryAddProviderSpecificServices(serviceMap =>
                serviceMap.TryAddSingleton<ITriggerSqlGenerator, PostgreSqlTriggerSqlGenerator>());
    }
}