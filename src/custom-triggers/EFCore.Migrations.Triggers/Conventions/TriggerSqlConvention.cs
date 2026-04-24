using System.Linq;
using EFCore.Migrations.CustomSql;
using EFCore.Migrations.Triggers.Abstractions;
using EFCore.Migrations.Triggers.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace EFCore.Migrations.Triggers.Conventions
{
    public class TriggerSqlConvention : IModelFinalizingConvention
    {
        private readonly ITriggerSqlGenerator _triggerSqlGenerator;

        private readonly IRelationalAnnotationProvider _annotationProvider;

        public TriggerSqlConvention(ITriggerSqlGenerator triggerSqlGenerator, RelationalConventionSetBuilderDependencies dependencies)
        {
            _triggerSqlGenerator = triggerSqlGenerator;
            _annotationProvider = dependencies.RelationalAnnotationProvider;
        }

        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var triggerAnnotations = entityType.GetAnnotations()
                    .Where(a => a.Value is TriggerObject)
                    .ToList();

                foreach (var annotation in triggerAnnotations)
                {
                    if (annotation.Value is not TriggerObject triggerData) continue;

                    var sqlUp = _triggerSqlGenerator.GenerateCreateTriggerSql(triggerData);
                    var sqpDown = _triggerSqlGenerator.GenerateDeleteTriggerSql(triggerData);

                    entityType.RemoveAnnotation(annotation.Name);

                    entityType.Builder.AddCustomSql(triggerData.Name, sqlUp, sqpDown);
                }
            }
        }
    }
}